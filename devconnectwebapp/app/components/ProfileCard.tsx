type ProfileCardProps = {
  profileName: string;
  profileAge: string;
  profileLoading: boolean;
  profileError: string | null;
  profileMessage: string | null;
  onProfileNameChange: (value: string) => void;
  onProfileAgeChange: (value: string) => void;
  onLoadProfile: () => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void;
};

export default function ProfileCard(props: ProfileCardProps) {
  return (
    <>
      <div>
        <h2 className="section-title">Profile</h2>
      </div>

      <form onSubmit={props.onSubmit} className="stack">
        <div className="field">
          <label className="label" htmlFor="profileName">
            Name
          </label>
          <input
            id="profileName"
            className="input"
            value={props.profileName}
            onChange={(event) => props.onProfileNameChange(event.target.value)}
          />
        </div>

        <div className="field">
          <label className="label" htmlFor="age">
            Age
          </label>
          <input
            id="age"
            className="input"
            type="number"
            min={0}
            value={props.profileAge}
            onChange={(event) => props.onProfileAgeChange(event.target.value)}
          />
        </div>

        <div className="row">
          <button type="button" className="btn btn-ghost" onClick={props.onLoadProfile}>
            Load profile
          </button>
          <button type="submit" className="btn btn-primary" disabled={props.profileLoading}>
            Save profile
          </button>
        </div>
      </form>

      {props.profileError ? <p className="message error">{props.profileError}</p> : null}
      {props.profileMessage ? <p className="message success">{props.profileMessage}</p> : null}
    </>
  );
}