"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ApiError, getApiBaseUrl } from "@/lib/api";
import { useAuth } from "@/app/providers/AuthProvider";

type AuthPageProps = {
  mode: "login" | "register";
  redirect: string;
};

export default function AuthPage({ mode, redirect }: AuthPageProps) {
  const router = useRouter();
  const { isAuthenticated, login, register } = useAuth();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isAuthenticated) {
      router.replace(redirect);
    }
  }, [isAuthenticated, redirect, router]);

  async function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      if (mode === "register") {
        await register({ name, email, password });
      } else {
        await login({ email, password });
      }

      router.push(redirect);
    } catch (err) {
      setError(getErrorText(err));
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className="auth-shell route-frame">
      <div className="panel auth-panel">
        <div className="stack">
          {/* <span className="tag">Connected to {getApiBaseUrl()}</span> */}
          <h1 className="auth-title">{mode === "login" ? "Welcome back" : "Create your developer account"}</h1>
          <p className="meta">
            {mode === "login"
              ? "Log in to read full posts, publish your own work, and join the conversation."
              : "Register once and you can create posts, like useful ideas, and comment on discussions."}
          </p>
        </div>

        <form className="stack" onSubmit={onSubmit}>
          {mode === "register" ? (
            <div className="field">
              <label className="label" htmlFor="auth-name">
                Name
              </label>
              <input
                id="auth-name"
                className="input"
                value={name}
                onChange={(event) => setName(event.target.value)}
                required
              />
            </div>
          ) : null}

          <div className="field">
            <label className="label" htmlFor="auth-email">
              Email
            </label>
            <input
              id="auth-email"
              className="input"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </div>

          <div className="field">
            <label className="label" htmlFor="auth-password">
              Password
            </label>
            <input
              id="auth-password"
              className="input"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </div>

          <button className="btn btn-primary" type="submit" disabled={loading}>
            {loading
              ? "Please wait..."
              : mode === "login"
                ? "Login"
                : "Create account"}
          </button>
        </form>

        {error ? <p className="message error">{error}</p> : null}

        <p className="meta">
          {mode === "login" ? "No account yet?" : "Already have an account?"}{" "}
          <Link href={mode === "login" ? "/register" : "/login"} className="inline-link">
            {mode === "login" ? "Register" : "Login"}
          </Link>
        </p>
      </div>
    </section>
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