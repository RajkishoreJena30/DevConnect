"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import ProtectedRoute from "@/app/components/ProtectedRoute";
import { useAuth } from "@/app/providers/AuthProvider";
import { ApiError, api } from "@/lib/api";
import {
  BookmarkSortBy,
  BookmarkStats,
  PagedResult,
  PostResponse,
  SortDirection,
} from "@/lib/types";

const initialFeed: PagedResult<PostResponse> = {
  items: [],
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
};

export default function BookmarksPage() {
  const { authData } = useAuth();
  const [feed, setFeed] = useState<PagedResult<PostResponse>>(initialFeed);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [sortBy, setSortBy] = useState<BookmarkSortBy>("createdAt");
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [busyPostId, setBusyPostId] = useState<number | null>(null);
  const [topBookmarked, setTopBookmarked] = useState<BookmarkStats[]>([]);

  useEffect(() => {
    if (!authData?.token) {
      return;
    }

    let active = true;
    setLoading(true);
    setError(null);

    api
      .getMyBookmarks(authData.token, {
        pageNumber,
        pageSize: 10,
        sortBy,
        sortDirection,
        search,
      })
      .then((paged) => {
        if (active) {
          setFeed(paged);
        }
      })
      .catch((err) => {
        if (active) {
          setError(getErrorText(err));
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [authData?.token, pageNumber, sortBy, sortDirection, search]);

  useEffect(() => {
    let active = true;
    api
      .getTopBookmarked(5)
      .then((stats) => {
        if (active) {
          setTopBookmarked(stats);
        }
      })
      .catch(() => {
        // Trending list is optional; ignore failures.
      });

    return () => {
      active = false;
    };
  }, [feed.totalCount]);

  async function onRemoveBookmark(postId: number) {
    if (!authData?.token) {
      return;
    }

    try {
      setBusyPostId(postId);
      await api.toggleBookmark(postId, authData.token);
      setFeed((current) => ({
        ...current,
        items: current.items.filter((item) => item.id !== postId),
        totalCount: Math.max(0, current.totalCount - 1),
      }));
    } catch (err) {
      setError(getErrorText(err));
    } finally {
      setBusyPostId(null);
    }
  }

  function onSubmitSearch(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPageNumber(1);
    setSearch(searchInput.trim());
  }

  const pageInfo = useMemo(
    () => `Page ${feed.pageNumber} of ${Math.max(feed.totalPages, 1)}`,
    [feed.pageNumber, feed.totalPages]
  );

  return (
    <ProtectedRoute>
      <section className="route-frame dashboard-grid">
        <div className="panel hero-card">
          <span className="tag">Your library</span>
          <h1 className="page-title">Saved posts</h1>
          <p className="page-copy">
            Every post you bookmark lands here. Search, sort, and revisit the discussions you care about.
          </p>
        </div>

        {topBookmarked.length ? (
          <article className="panel card">
            <h2 className="section-title">Most saved</h2>
            <div className="stack">
              {topBookmarked.map((item) => (
                <div className="row" key={item.postId} style={{ justifyContent: "space-between" }}>
                  <Link className="nav-link" href={`/posts/${item.postId}`}>
                    {item.title}
                  </Link>
                  <span className="tag">{item.bookmarkCount} saves</span>
                </div>
              ))}
            </div>
          </article>
        ) : null}

        <article className="panel card">
          <div className="row" style={{ justifyContent: "space-between" }}>
            <h2 className="section-title">Bookmarks</h2>
            <span className="tag">{feed.totalCount} saved</span>
          </div>

          <form onSubmit={onSubmitSearch} className="row" style={{ marginBottom: "0.8rem" }}>
            <input
              className="input"
              placeholder="Search saved posts..."
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
            />
            <button type="submit" className="btn btn-primary">
              Search
            </button>
          </form>

          <div className="row" style={{ marginBottom: "0.8rem" }}>
            <label className="label" htmlFor="bookmarkSort">
              Sort by
            </label>
            <select
              id="bookmarkSort"
              className="select"
              value={sortBy}
              onChange={(event) => {
                setPageNumber(1);
                setSortBy(event.target.value as BookmarkSortBy);
              }}
            >
              <option value="createdAt">Created</option>
              <option value="title">Title</option>
            </select>

            <label className="label" htmlFor="bookmarkDirection">
              Direction
            </label>
            <select
              id="bookmarkDirection"
              className="select"
              value={sortDirection}
              onChange={(event) => {
                setPageNumber(1);
                setSortDirection(event.target.value as SortDirection);
              }}
            >
              <option value="desc">Desc</option>
              <option value="asc">Asc</option>
            </select>
          </div>

          {error ? <p className="message error">{error}</p> : null}
          {loading ? <p className="message">Loading bookmarks...</p> : null}
          {!loading && feed.items.length === 0 ? (
            <p className="message">No saved posts yet. Tap Save on a post to add it here.</p>
          ) : null}

          {feed.items.map((post) => (
            <div className="post" key={post.id}>
              <h3>{post.title}</h3>
              <p className="meta">
                By {post.authorName} | {new Date(post.createdAt).toLocaleString()}
              </p>
              <p className="content">{truncate(post.content, 220)}</p>
              <div className="row">
                <Link className="btn btn-primary" href={`/posts/${post.id}`}>
                  Read more
                </Link>
                <button
                  type="button"
                  className="btn btn-ghost"
                  onClick={() => void onRemoveBookmark(post.id)}
                  disabled={busyPostId === post.id}
                >
                  {"\u2605 Remove"}
                </button>
              </div>
            </div>
          ))}

          <div className="row" style={{ justifyContent: "space-between", marginTop: "1rem" }}>
            <span className="meta">{pageInfo}</span>
            <div className="row">
              <button
                className="btn btn-ghost"
                type="button"
                disabled={pageNumber <= 1}
                onClick={() => setPageNumber((current) => Math.max(1, current - 1))}
              >
                Previous
              </button>
              <button
                className="btn btn-ghost"
                type="button"
                disabled={pageNumber >= feed.totalPages}
                onClick={() => setPageNumber((current) => current + 1)}
              >
                Next
              </button>
            </div>
          </div>
        </article>
      </section>
    </ProtectedRoute>
  );
}

function truncate(value: string, maxLength: number): string {
  if (value.length <= maxLength) {
    return value;
  }

  return `${value.slice(0, maxLength).trimEnd()}...`;
}

function getErrorText(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status === 401) {
      return "Unauthorized. Please login again.";
    }
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Something went wrong. Please try again.";
}
