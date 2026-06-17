"use client";

import { useEffect, useMemo, useState } from "react";
import ProtectedRoute from "@/app/components/ProtectedRoute";
import CreatePostCard from "@/app/components/CreatePostCard";
import FeedCard from "@/app/components/FeedCard";
import { useAuth } from "@/app/providers/AuthProvider";
import { ApiError, api } from "@/lib/api";
import {
  CommentResponse,
  LikeResponse,
  PagedResult,
  PostResponse,
  SortBy,
  SortDirection,
} from "@/lib/types";

const initialFeed: PagedResult<PostResponse> = {
  items: [],
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
};

export default function DashboardPage() {
  const { authData } = useAuth();
  const [postTitle, setPostTitle] = useState("");
  const [postContent, setPostContent] = useState("");
  const [postMessage, setPostMessage] = useState<string | null>(null);
  const [postError, setPostError] = useState<string | null>(null);
  const [postLoading, setPostLoading] = useState(false);
  const [feed, setFeed] = useState<PagedResult<PostResponse>>(initialFeed);
  const [feedLoading, setFeedLoading] = useState(false);
  const [feedError, setFeedError] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [sortBy, setSortBy] = useState<SortBy>("createdAt");
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");
  const [likesByPostId, setLikesByPostId] = useState<Record<number, LikeResponse>>({});
  const [commentsByPostId, setCommentsByPostId] = useState<Record<number, CommentResponse[]>>({});
  const [commentsOpen, setCommentsOpen] = useState<Record<number, boolean>>({});
  const [commentDrafts, setCommentDrafts] = useState<Record<number, string>>({});
  const [busyCommentPostId, setBusyCommentPostId] = useState<number | null>(null);
  const [busyLikePostId, setBusyLikePostId] = useState<number | null>(null);

  useEffect(() => {
    void loadFeed();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, sortBy, sortDirection, authData?.token]);

  async function loadFeed() {
    try {
      setFeedLoading(true);
      setFeedError(null);
      const paged = await api.getPosts({
        pageNumber,
        pageSize: 10,
        sortBy,
        sortDirection,
      });
      setFeed(paged);

      const entries = await Promise.all(
        paged.items.map(async (post) => [post.id, await api.getLikes(post.id, authData?.token)] as const)
      );
      setLikesByPostId(Object.fromEntries(entries));
    } catch (error) {
      setFeedError(getErrorText(error));
    } finally {
      setFeedLoading(false);
    }
  }

  async function onCreatePost(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!authData?.token) {
      setPostError("Please log in to create a post.");
      return;
    }

    try {
      setPostLoading(true);
      setPostError(null);
      setPostMessage(null);
      await api.createPost(authData.token, { title: postTitle, content: postContent });
      setPostTitle("");
      setPostContent("");
      setPostMessage("Post created.");
      setPageNumber(1);
      await loadFeed();
    } catch (error) {
      setPostError(getErrorText(error));
    } finally {
      setPostLoading(false);
    }
  }

  async function onToggleLike(postId: number) {
    if (!authData?.token) {
      setFeedError("Please log in to like posts.");
      return;
    }

    try {
      setBusyLikePostId(postId);
      const updated = await api.toggleLike(postId, authData.token);
      setLikesByPostId((current) => ({ ...current, [postId]: updated }));
    } catch (error) {
      setFeedError(getErrorText(error));
    } finally {
      setBusyLikePostId(null);
    }
  }

  async function onToggleComments(postId: number) {
    setCommentsOpen((current) => ({ ...current, [postId]: !current[postId] }));

    if (commentsByPostId[postId]) {
      return;
    }

    try {
      setBusyCommentPostId(postId);
      const comments = await api.getComments(postId);
      setCommentsByPostId((current) => ({ ...current, [postId]: comments }));
    } catch (error) {
      setFeedError(getErrorText(error));
    } finally {
      setBusyCommentPostId(null);
    }
  }

  async function onSubmitComment(postId: number, event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!authData?.token) {
      setFeedError("Please log in to comment.");
      return;
    }

    const draft = (commentDrafts[postId] ?? "").trim();
    if (!draft) {
      return;
    }

    try {
      setBusyCommentPostId(postId);
      const created = await api.addComment(postId, authData.token, { content: draft });
      setCommentsByPostId((current) => ({ ...current, [postId]: [created, ...(current[postId] ?? [])] }));
      setCommentDrafts((current) => ({ ...current, [postId]: "" }));
    } catch (error) {
      setFeedError(getErrorText(error));
    } finally {
      setBusyCommentPostId(null);
    }
  }

  const pageInfo = useMemo(
    () => `Page ${feed.pageNumber} of ${Math.max(feed.totalPages, 1)}`,
    [feed.pageNumber, feed.totalPages]
  );

  return (
    <ProtectedRoute>
      <section className="route-frame dashboard-grid">
        <div className="panel hero-card">
          <span className="tag">Private workspace</span>
          <h1 className="page-title">Developer dashboard</h1>
          <p className="page-copy">
            Publish your ideas, scan recent discussions, and jump into the full post view when you want the complete context.
          </p>
        </div>

        <CreatePostCard
          postTitle={postTitle}
          postContent={postContent}
          postLoading={postLoading}
          postError={postError}
          postMessage={postMessage}
          onPostTitleChange={setPostTitle}
          onPostContentChange={setPostContent}
          onSubmit={onCreatePost}
        />

        <FeedCard
          feed={feed}
          feedError={feedError}
          feedLoading={feedLoading}
          sortBy={sortBy}
          sortDirection={sortDirection}
          pageNumber={pageNumber}
          pageInfo={pageInfo}
          likesByPostId={likesByPostId}
          commentsByPostId={commentsByPostId}
          commentsOpen={commentsOpen}
          commentDrafts={commentDrafts}
          busyCommentPostId={busyCommentPostId}
          busyLikePostId={busyLikePostId}
          onSortByChange={(value) => {
            setPageNumber(1);
            setSortBy(value);
          }}
          onSortDirectionChange={(value) => {
            setPageNumber(1);
            setSortDirection(value);
          }}
          onRefresh={() => void loadFeed()}
          onToggleLike={onToggleLike}
          onToggleComments={onToggleComments}
          onCommentDraftChange={(postId, value) =>
            setCommentDrafts((current) => ({ ...current, [postId]: value }))
          }
          onSubmitComment={onSubmitComment}
          onPreviousPage={() => setPageNumber((current) => Math.max(1, current - 1))}
          onNextPage={() => setPageNumber((current) => current + 1)}
          formatDate={(value) => new Date(value).toLocaleString()}
        />
      </section>
    </ProtectedRoute>
  );
}

function getErrorText(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status === 401) {
      return "Unauthorized. Please login again.";
    }
    if (error.status === 403) {
      return "You are not allowed to do this action.";
    }
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Something went wrong. Please try again.";
}