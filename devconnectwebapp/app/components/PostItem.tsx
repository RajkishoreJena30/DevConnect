import Link from "next/link";
import { CommentResponse, LikeResponse, PostResponse } from "@/lib/types";

type PostItemProps = {
  post: PostResponse;
  likes: LikeResponse;
  comments: CommentResponse[];
  isCommentsOpen: boolean;
  commentDraft: string;
  busyCommentPostId: number | null;
  busyLikePostId: number | null;
  isBookmarked: boolean;
  busyBookmarkPostId: number | null;
  onToggleLike: (postId: number) => Promise<void>;
  onToggleBookmark: (postId: number) => Promise<void>;
  onToggleComments: (postId: number) => Promise<void>;
  onCommentDraftChange: (postId: number, value: string) => void;
  onSubmitComment: (postId: number, event: React.FormEvent<HTMLFormElement>) => Promise<void>;
  formatDate: (value: string) => string;
  detailHref: string;
};

export default function PostItem(props: PostItemProps) {
  const { post } = props;

  return (
    <div className="post" key={post.id}>
      <h3>{post.title}</h3>
      <p className="meta">
        By {post.authorName} | {props.formatDate(post.createdAt)}
      </p>
      <p className="content">{truncate(post.content, 220)}</p>

      <div className="row">
        <Link className="btn btn-primary" href={props.detailHref}>
          Read more
        </Link>
        <button
          className="btn btn-ghost"
          type="button"
          onClick={() => void props.onToggleLike(post.id)}
          disabled={props.busyLikePostId === post.id}
        >
          {props.likes.likedByMe ? "Unlike" : "Like"} ({props.likes.totalLikes})
        </button>

        <button
          className="btn btn-ghost"
          type="button"
          onClick={() => void props.onToggleBookmark(post.id)}
          disabled={props.busyBookmarkPostId === post.id}
        >
          {props.isBookmarked ? "\u2605 Saved" : "\u2606 Save"}
        </button>

        <button
          className="btn btn-ghost"
          type="button"
          onClick={() => void props.onToggleComments(post.id)}
          disabled={props.busyCommentPostId === post.id}
        >
          {props.isCommentsOpen ? "Hide comments" : "Show comments"} ({props.comments.length || post.commentsCount})
        </button>
      </div>

      {props.isCommentsOpen ? (
        <div className="stack">
          {props.comments.map((comment) => (
            <div className="comment" key={comment.id}>
              <strong>{comment.authorName}</strong>
              <span className="meta"> | {props.formatDate(comment.createdAt)}</span>
              <p>{comment.content}</p>
            </div>
          ))}

          <form onSubmit={(event) => void props.onSubmitComment(post.id, event)} className="stack">
            <textarea
              className="textarea"
              placeholder="Write a comment..."
              value={props.commentDraft}
              onChange={(event) => props.onCommentDraftChange(post.id, event.target.value)}
            />
            <button
              type="submit"
              className="btn btn-primary"
              disabled={props.busyCommentPostId === post.id}
            >
              Add comment
            </button>
          </form>
        </div>
      ) : null}
    </div>
  );
}

function truncate(value: string, maxLength: number): string {
  if (value.length <= maxLength) {
    return value;
  }

  return `${value.slice(0, maxLength).trimEnd()}...`;
}