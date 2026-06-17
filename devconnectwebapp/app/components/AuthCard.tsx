export type AuthMode = "login" | "register";

type AuthCardProps = {
  authMode: AuthMode;
  authLoading: boolean;
  authError: string | null;
  authMessage: string | null;
  isAuthenticated: boolean;
  loggedInName?: string;
  apiBaseUrl: string;
  email: string;
  password: string;
  name: string;
  onChangeMode: (mode: AuthMode) => void;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onNameChange: (value: string) => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void;
  onLogout: () => void;
};

export default function AuthCard(props: AuthCardProps) {
  return (
    <>
      <div>
        <h2 className="section-title">Auth</h2>
        <p className="meta">API base: {props.apiBaseUrl}</p>
      </div>

      <div className="row">
        <button
          className="btn btn-ghost"
          onClick={() => props.onChangeMode("login")}
          type="button"
        >
          Login
        </button>
        <button
          className="btn btn-ghost"
          onClick={() => props.onChangeMode("register")}
          type="button"
        >
          Register
        </button>
      </div>

      <form onSubmit={props.onSubmit} className="stack">
        {props.authMode === "register" ? (
          <div className="field">
            <label className="label" htmlFor="name">
              Name
            </label>
            <input
              id="name"
              className="input"
              value={props.name}
              onChange={(event) => props.onNameChange(event.target.value)}
              required
            />
          </div>
        ) : null}

        <div className="field">
          <label className="label" htmlFor="email">
            Email
          </label>
          <input
            id="email"
            className="input"
            type="email"
            value={props.email}
            onChange={(event) => props.onEmailChange(event.target.value)}
            required
          />
        </div>

        <div className="field">
          <label className="label" htmlFor="password">
            Password
          </label>
          <input
            id="password"
            className="input"
            type="password"
            value={props.password}
            onChange={(event) => props.onPasswordChange(event.target.value)}
            required
          />
        </div>

        <button className="btn btn-primary" type="submit" disabled={props.authLoading}>
          {props.authLoading
            ? "Please wait..."
            : props.authMode === "register"
              ? "Create account"
              : "Login"}
        </button>
      </form>

      {props.isAuthenticated ? (
        <div className="stack">
          <span className="tag">Logged in as {props.loggedInName}</span>
          <button type="button" className="btn btn-danger" onClick={props.onLogout}>
            Logout
          </button>
        </div>
      ) : null}

      {props.authError ? <p className="message error">{props.authError}</p> : null}
      {props.authMessage ? <p className="message success">{props.authMessage}</p> : null}
    </>
  );
}