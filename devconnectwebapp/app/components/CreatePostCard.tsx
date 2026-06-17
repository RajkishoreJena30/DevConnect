type CreatePostCardProps = {
  postTitle: string;
  postContent: string;
  postLoading: boolean;
  postError: string | null;
  postMessage: string | null;
  onPostTitleChange: (value: string) => void;
  onPostContentChange: (value: string) => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void;
};

export default function CreatePostCard(props: CreatePostCardProps) {
  return (
    <article className="panel card">
      <h2 className="section-title">Create Post</h2>
      <form onSubmit={props.onSubmit} className="stack">
        <div className="field">
          <label className="label" htmlFor="postTitle">
            Title
          </label>
          <input
            id="postTitle"
            className="input"
            value={props.postTitle}
            onChange={(event) => props.onPostTitleChange(event.target.value)}
            required
          />
        </div>

        <div className="field">
          <label className="label" htmlFor="postContent">
            Content
          </label>
          <textarea
            id="postContent"
            className="textarea"
            value={props.postContent}
            onChange={(event) => props.onPostContentChange(event.target.value)}
            required
          />
        </div>

        <button className="btn btn-primary" type="submit" disabled={props.postLoading}>
          {props.postLoading ? "Publishing..." : "Publish post"}
        </button>
      </form>
      {props.postError ? <p className="message error">{props.postError}</p> : null}
      {props.postMessage ? <p className="message success">{props.postMessage}</p> : null}
    </article>
  );
}