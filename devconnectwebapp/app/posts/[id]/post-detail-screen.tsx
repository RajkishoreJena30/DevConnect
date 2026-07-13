"use client";

import { useEffect, useState } from "react";
import ProtectedRoute from "@/app/components/ProtectedRoute";
import { useAuth } from "@/app/providers/AuthProvider";
import { ApiError, api } from "@/lib/api";
import { CommentResponse, LikeResponse, PostResponse } from "@/lib/types";

export default function PostDetailScreen({ postId }: { postId: number }) {
  const { authData } = useAuth();
  const [post, setPost] = useState<PostResponse | null>(null);
  const [likes, setLikes] = useState<LikeResponse | null>(null);
  const [comments, setComments] = useState<CommentResponse[]>([]);
  const [bookmarked, setBookmarked] = useState(false);
  const [commentDraft, setCommentDraft] = useState("");
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    setLoading(true);
    setError(null);

    Promise.all([
      api.getPostById(postId),
      api.getLikes(postId, authData?.token),
      api.getComments(postId),
      authData?.token
        ? api
            .getMyBookmarks(authData.token, {
              pageNumber: 1,
              pageSize: 100,
              sortBy: "createdAt",
              sortDirection: "desc",
            })
            .then((result) => result.items.some((item) => item.id === postId))
            .catch(() => false)
        : Promise.resolve(false),
    ])
      .then(([postResponse, likesResponse, commentsResponse, bookmarkedResponse]) => {
        if (!active) {
          return;
        }

        setPost(postResponse);
        setLikes(likesResponse);
        setComments(commentsResponse);
        setBookmarked(bookmarkedResponse);
      })
      .catch((err) => {
        if (!active) {
          return;
        }

        setError(getErrorText(err));
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [postId, authData?.token]);

  async function onToggleLike() {
    if (!authData?.token) {
      setError("Please login to like this post.");
      return;
    }

    try {
      setSubmitting(true);
      const updated = await api.toggleLike(postId, authData.token);
      setLikes(updated);
    } catch (err) {
      setError(getErrorText(err));
    } finally {
      setSubmitting(false);
    }
  }

  async function onToggleBookmark() {
    if (!authData?.token) {
      setError("Please login to save this post.");
      return;
    }

    try {
      setSubmitting(true);
      const result = await api.toggleBookmark(postId, authData.token);
      setBookmarked(result.bookmarked);
    } catch (err) {
      setError(getErrorText(err));
    } finally {
      setSubmitting(false);
    }
  }

  async function onSubmitComment(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!authData?.token) {
      setError("Please login to comment on this post.");
      return;
    }

    const draft = commentDraft.trim();
    if (!draft) {
      return;
    }

    try {
      setSubmitting(true);
      const created = await api.addComment(postId, authData.token, { content: draft });
      setComments((current) => [created, ...current]);
      setCommentDraft("");
    } catch (err) {
      setError(getErrorText(err));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <ProtectedRoute>
      <section className="route-frame detail-shell">
        {loading ? (
          <div className="panel card">
            <p className="message">Loading post details...</p>
          </div>
        ) : null}

        {error ? <div className="panel card"><p className="message error">{error}</p></div> : null}

        {post ? (
          <>
            <article className="panel detail-card">
              <div className="row">
                <span className="tag">{likes?.totalLikes ?? post.likesCount} likes</span>
                <span className="tag">{comments.length || post.commentsCount} comments</span>
              </div>

              <h1 className="page-title">{post.title}</h1>
              <p className="meta">
                By {post.authorName} | {new Date(post.createdAt).toLocaleString()}
              </p>
              <p className="detail-content">{post.content}</p>

              <div className="row">
                <button type="button" className="btn btn-ghost" onClick={() => void onToggleLike()} disabled={submitting}>
                  {likes?.likedByMe ? "Unlike" : "Like"} ({likes?.totalLikes ?? post.likesCount})
                </button>
                <button type="button" className="btn btn-ghost" onClick={() => void onToggleBookmark()} disabled={submitting}>
                  {bookmarked ? "\u2605 Saved" : "\u2606 Save"}
                </button>
              </div>
            </article>

            <article className="panel card stack">
              <h2 className="section-title">Comments</h2>

              <form onSubmit={onSubmitComment} className="stack">
                <textarea
                  className="textarea"
                  value={commentDraft}
                  onChange={(event) => setCommentDraft(event.target.value)}
                  placeholder="Add a thoughtful response..."
                />
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  Add comment
                </button>
              </form>

              {comments.map((comment) => (
                <div className="comment" key={comment.id}>
                  <strong>{comment.authorName}</strong>
                  <span className="meta"> | {new Date(comment.createdAt).toLocaleString()}</span>
                  <p>{comment.content}</p>
                </div>
              ))}
            </article>
          </>
        ) : null}
      </section>
    </ProtectedRoute>
  );
}

function getErrorText(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Something went wrong. Please try again.";
}