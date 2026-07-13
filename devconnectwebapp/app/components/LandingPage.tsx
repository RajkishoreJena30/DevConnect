"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import PublicPostPreview from "@/app/components/PublicPostPreview";
import { useAuth } from "@/app/providers/AuthProvider";
import { ApiError, api } from "@/lib/api";
import { PagedResult, PostResponse } from "@/lib/types";

const highlightItems = [
  {
    title: "Technical publishing",
    description: "Turn release notes, architecture ideas, and debugging lessons into readable posts.",
  },
  {
    title: "Focused discussion",
    description: "Keep comments attached to the post so implementation context is never lost.",
  },
  {
    title: "Developer signal",
    description: "Likes help the most useful content surface without turning the product into noise.",
  },
];

const workflowSteps = [
  "Create an account and publish your first engineering update.",
  "Browse recent posts from other developers and open full discussions.",
  "Like strong ideas and comment with your own implementation experience.",
];

const emptyFeed: PagedResult<PostResponse> = {
  items: [],
  totalCount: 0,
  pageNumber: 1,
  pageSize: 3,
  totalPages: 1,
};

export default function LandingPage() {
  const { isAuthenticated } = useAuth();
  const [feed, setFeed] = useState<PagedResult<PostResponse>>(emptyFeed);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;

    api
      .getPosts({
        pageNumber: 1,
        pageSize: 3,
        sortBy: "createdAt",
        sortDirection: "desc",
      })
      .then((response) => {
        if (active) {
          setFeed(response);
          setError(null);
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
  }, []);

  return (
    <main className="route-frame marketing-shell">
      <section className="hero-surface hero-surface--rich panel">
        <div className="hero-grid">
          <div className="stack hero-copy">
            <span className="tag hero-tag">Built for developers</span>
            <h1 className="landing-title">A better place to publish engineering work and discuss it with other developers.</h1>
            <p className="landing-copy">
              DevConnect gives your backend notes, frontend lessons, and product delivery updates a dedicated home. Public visitors can preview the platform, while signed-in developers unlock the full reading and discussion experience.
            </p>

            <div className="hero-metrics">
              <div className="metric-chip">
                <strong>{feed.totalCount || 0}</strong>
                <span>posts available</span>
              </div>
              <div className="metric-chip">
                <strong>Full threads</strong>
                <span>for signed-in members</span>
              </div>
              <div className="metric-chip">
                <strong>Fast publish</strong>
                <span>from your dashboard</span>
              </div>
            </div>

            <div className="row hero-actions">
              {isAuthenticated ? (
                <Link className="btn btn-primary" href="/dashboard">
                  Go to dashboard
                </Link>
              ) : (
                <>
                  <Link className="btn btn-primary" href="/register">
                    Start publishing
                  </Link>
                  <Link className="btn btn-ghost" href="/login">
                    Login
                  </Link>
                </>
              )}
            </div>
          </div>

          <div className="hero-showcase">
            <div className="hero-showcase__card panel">
              <div className="hero-showcase__header">
                <span className="tag">Community pulse</span>
                <span className="meta">Public preview</span>
              </div>

              <div className="hero-showcase__body stack">
                <div>
                  <p className="meta">Why teams use this</p>
                  <h2 className="hero-showcase__title">Share updates that read like product thinking, not scattered chat messages.</h2>
                </div>

                <div className="signal-list">
                  <div className="signal-item">
                    <strong>Clear author identity</strong>
                    <p className="meta">Every post is anchored to a developer profile and visible activity.</p>
                  </div>
                  <div className="signal-item">
                    <strong>Protected full content</strong>
                    <p className="meta">Visitors see enough to understand the platform, members get the full post and thread.</p>
                  </div>
                  <div className="signal-item">
                    <strong>Better conversations</strong>
                    <p className="meta">Likes and comments stay attached to the original idea instead of getting lost.</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="feature-strip stack">
        <div className="section-header-block">
          <span className="tag">Platform focus</span>
          <h2 className="landing-section-title">Designed around how developers actually share work</h2>
        </div>

        <div className="feature-grid feature-grid--elevated">
          {highlightItems.map((item) => (
            <article key={item.title} className="panel feature-card">
              <div className="feature-card__icon" aria-hidden="true">
                <span />
              </div>
              <h3>{item.title}</h3>
              <p className="meta">{item.description}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="panel workflow-band">
        <div className="workflow-band__intro">
          <span className="tag">How it works</span>
          <h2 className="landing-section-title">From public preview to full developer workspace</h2>
          <p className="meta">The homepage introduces the community. Authentication unlocks the complete product.</p>
        </div>

        <div className="workflow-steps">
          {workflowSteps.map((step, index) => (
            <div key={step} className="workflow-step">
              <span className="workflow-step__index">0{index + 1}</span>
              <p>{step}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="stack preview-section">
        <div className="section-header-row">
          <div className="stack preview-section__heading">
            <span className="tag">Recent writing</span>
            <div>
              <h2 className="landing-section-title">Preview the latest conversations</h2>
              <p className="meta">Signed-in users can open the full post, like it, and join the discussion.</p>
            </div>
          </div>
          <span className="preview-count">{feed.items.length} featured previews</span>
          <Link className="btn btn-ghost" href={isAuthenticated ? "/dashboard" : "/login"}>
            {isAuthenticated ? "Read more" : "Login to read more"}
          </Link>
        </div>

        {loading ? <p className="message">Loading recent posts...</p> : null}
        {error ? <p className="message error">{error}</p> : null}

        <div className="preview-grid">
          {feed.items.map((post) => (
            <PublicPostPreview key={post.id} post={post} href={`/posts/${post.id}`} />
          ))}
        </div>
      </section>

      <section className="panel cta-banner">
        <div className="cta-banner__copy">
          <span className="tag">Ready to join?</span>
          <h2 className="landing-section-title">Create an account and make DevConnect your developer publishing space.</h2>
          <p className="meta">Write posts, keep useful feedback, and let other developers build on your ideas.</p>
        </div>

        <div className="row">
          <Link className="btn btn-primary" href="/register">
            Create account
          </Link>
          <Link className="btn btn-ghost" href="/dashboard">
            View member workspace
          </Link>
        </div>
      </section>
    </main>
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