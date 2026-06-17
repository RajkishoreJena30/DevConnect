"use client";

import Link from "next/link";
import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/app/providers/AuthProvider";

export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (!isAuthenticated) {
      router.replace(`/login?redirect=${encodeURIComponent(pathname || "/dashboard")}`);
    }
  }, [isAuthenticated, pathname, router]);

  if (!isAuthenticated) {
    return (
      <section className="route-frame">
        <div className="panel card protected-empty">
          <h1 className="section-title">Login required</h1>
          <p className="meta">You need an account to access this page.</p>
          <div className="row">
            <Link className="btn btn-primary" href={`/login?redirect=${encodeURIComponent(pathname || "/dashboard")}`}>
              Login
            </Link>
            <Link className="btn btn-ghost" href="/register">
              Register
            </Link>
          </div>
        </div>
      </section>
    );
  }

  return <>{children}</>;
}