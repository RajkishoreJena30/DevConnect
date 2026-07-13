import {
  CommentResponse,
  LikeResponse,
  PagedResult,
  PostResponse,
  SortBy,
  SortDirection,
} from "@/lib/types";
import PostItem from "@/app/components/PostItem";

type FeedCardProps = {
  feed: PagedResult<PostResponse>;
  feedError: string | null;
  feedLoading: boolean;
  sortBy: SortBy;
  sortDirection: SortDirection;
  pageNumber: number;
  pageInfo: string;
  likesByPostId: Record<number, LikeResponse>;
  commentsByPostId: Record<number, CommentResponse[]>;
  commentsOpen: Record<number, boolean>;
  commentDrafts: Record<number, string>;
  busyCommentPostId: number | null;
  busyLikePostId: number | null;
  bookmarkedByPostId: Record<number, boolean>;
  busyBookmarkPostId: number | null;
  onSortByChange: (value: SortBy) => void;
  onSortDirectionChange: (value: SortDirection) => void;
  onRefresh: () => void;
  onToggleLike: (postId: number) => Promise<void>;
  onToggleBookmark: (postId: number) => Promise<void>;
  onToggleComments: (postId: number) => Promise<void>;
  onCommentDraftChange: (postId: number, value: string) => void;
  onSubmitComment: (postId: number, event: React.FormEvent<HTMLFormElement>) => Promise<void>;
  onPreviousPage: () => void;
  onNextPage: () => void;
  formatDate: (value: string) => string;
};

export default function FeedCard(props: FeedCardProps) {
  return (
    <article className="panel card">
      <div className="row" style={{ justifyContent: "space-between" }}>
        <h2 className="section-title">Developer Feed</h2>
        <span className="tag">{props.feed.totalCount} posts</span>
      </div>

      <div className="row" style={{ marginBottom: "0.8rem" }}>
        <label className="label" htmlFor="sortBy">
          Sort by
        </label>
        <select
          id="sortBy"
          className="select"
          value={props.sortBy}
          onChange={(event) => props.onSortByChange(event.target.value as SortBy)}
        >
          <option value="createdAt">Created</option>
          <option value="title">Title</option>
          <option value="likes">Likes</option>
        </select>

        <label className="label" htmlFor="direction">
          Direction
        </label>
        <select
          id="direction"
          className="select"
          value={props.sortDirection}
          onChange={(event) => props.onSortDirectionChange(event.target.value as SortDirection)}
        >
          <option value="desc">Desc</option>
          <option value="asc">Asc</option>
        </select>

        <button type="button" className="btn btn-ghost" onClick={props.onRefresh}>
          Refresh
        </button>
      </div>

      {props.feedError ? <p className="message error">{props.feedError}</p> : null}
      {props.feedLoading ? <p className="message">Loading feed...</p> : null}

      {props.feed.items.map((post) => (
        <PostItem
          key={post.id}
          post={post}
          likes={props.likesByPostId[post.id] ?? { totalLikes: post.likesCount, likedByMe: false }}
          comments={props.commentsByPostId[post.id] ?? []}
          isCommentsOpen={props.commentsOpen[post.id] ?? false}
          commentDraft={props.commentDrafts[post.id] ?? ""}
          busyCommentPostId={props.busyCommentPostId}
          busyLikePostId={props.busyLikePostId}
          isBookmarked={props.bookmarkedByPostId[post.id] ?? false}
          busyBookmarkPostId={props.busyBookmarkPostId}
          onToggleLike={props.onToggleLike}
          onToggleBookmark={props.onToggleBookmark}
          onToggleComments={props.onToggleComments}
          onCommentDraftChange={props.onCommentDraftChange}
          onSubmitComment={props.onSubmitComment}
          formatDate={props.formatDate}
          detailHref={`/posts/${post.id}`}
        />
      ))}

      <div className="row" style={{ justifyContent: "space-between", marginTop: "1rem" }}>
        <span className="meta">{props.pageInfo}</span>
        <div className="row">
          <button
            className="btn btn-ghost"
            type="button"
            disabled={props.pageNumber <= 1}
            onClick={props.onPreviousPage}
          >
            Previous
          </button>
          <button
            className="btn btn-ghost"
            type="button"
            disabled={props.pageNumber >= props.feed.totalPages}
            onClick={props.onNextPage}
          >
            Next
          </button>
        </div>
      </div>
    </article>
  );
}