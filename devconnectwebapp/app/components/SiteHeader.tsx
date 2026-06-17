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
                className={`nav-link ${isActive("/profile") ? "is-active" : ""}`}
                href="/profile"
              >
                Profile
              </Link>
              <span className="nav-badge">{authData?.name}</span>
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