"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/app/providers/AuthProvider";

export default function SiteHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const { authData, isAuthenticated, logout } = useAuth();

  function isActive(href: string): boolean {
    return pathname === href;
  }

  function handleLogout() {
    logout();
    router.push("/");
  }

  return (
    <header className="site-header">
      <div className="site-header-inner">
        <Link href="/" className="brand-mark">
          <span className="brand-kicker">DevConnect</span>
          <strong>Developer stories, discussions, and practical knowledge</strong>
        </Link>

        <nav className="site-nav">
          <Link className={`nav-link ${isActive("/") ? "is-active" : ""}`} href="/">
            Home
          </Link>

          {isAuthenticated ? (
            <>
              <Link
                className={`nav-link ${isActive("/dashboard") ? "is-active" : ""}`}
                href="/dashboard"
              >
                Dashboard
              </Link>
              <Link
                className={`nav-link ${isActive("/bookmarks") ? "is-active" : ""}`}
                href="/bookmarks"
              >
                Bookmarks
              </Link>
              <Link
                className={`nav-link ${isActive("/profile") ? "is-active" : ""}`}
                href="/profile"
              >
                Profile
              </Link>
              <Link
                className="nav-badge nav-user"
                href="/profile"
                style={{ marginLeft: "auto" }}
                title={authData?.name}
              >
                <svg
                  className="nav-user__icon"
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  aria-hidden="true"
                >
                  <circle cx="12" cy="8" r="4" />
                  <path d="M4 20c0-4 3.6-6 8-6s8 2 8 6" />
                </svg>
                <span className="nav-user__name">{authData?.name}</span>
              </Link>
              <button type="button" className="btn btn-ghost" onClick={handleLogout}>
                Logout
              </button>
            </>
          ) : (
            <>
              <Link
                className={`nav-link ${isActive("/login") ? "is-active" : ""}`}
                href="/login"
              >
                Login
              </Link>
              <Link className="btn btn-primary" href="/register">
                Create account
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  );
}