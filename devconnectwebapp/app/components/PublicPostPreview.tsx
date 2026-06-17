import Link from "next/link";
import { PostResponse } from "@/lib/types";

type PublicPostPreviewProps = {
  post: PostResponse;
  href: string;
};

export default function PublicPostPreview({ post, href }: PublicPostPreviewProps) {
  return (
    <article className="panel preview-card">
      <div className="stack preview-card__inner">
        <div className="preview-card__top row">
          <span className="tag">{post.likesCount} likes</span>
          <span className="tag">{post.commentsCount} comments</span>
        </div>

        <div>
          <h3 className="preview-title">{post.title}</h3>
          <p className="meta">
            By {post.authorName} | {formatDate(post.createdAt)}
          </p>
        </div>

        <p className="content preview-copy">{truncate(post.content, 180)}</p>

        <div className="preview-card__footer row">
          <Link className="btn btn-primary" href={href}>
            Read more
          </Link>
        </div>
      </div>
    </article>
  );
}

function truncate(value: string, maxLength: number): string {
  if (value.length <= maxLength) {
    return value;
  }

  return `${value.slice(0, maxLength).trimEnd()}...`;
}

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString();
}