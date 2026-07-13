"use client";

import { useEffect, useState } from "react";
import ProtectedRoute from "@/app/components/ProtectedRoute";
import ProfileCard from "@/app/components/ProfileCard";
import { useAuth } from "@/app/providers/AuthProvider";
import { ApiError, api } from "@/lib/api";

export default function ProfilePage() {
  const { authData, updateAuthName } = useAuth();
  const [profileName, setProfileName] = useState(authData?.name ?? "");
  const [profileAge, setProfileAge] = useState("0");
  const [profileLoading, setProfileLoading] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);
  const [profileMessage, setProfileMessage] = useState<string | null>(null);

  async function fetchProfile() {
    if (!authData?.token) {
      return;
    }

    try {
      setProfileLoading(true);
      setProfileError(null);
      setProfileMessage(null);
      const profile = await api.getProfile(authData.token);
      setProfileName(profile.name);
      setProfileAge(String(profile.age));
      setProfileMessage("Profile loaded.");
    } catch (error) {
      setProfileError(getErrorText(error));
    } finally {
      setProfileLoading(false);
    }
  }

  useEffect(() => {
    if (!authData?.token) {
      return;
    }

    let active = true;
    api
      .getProfile(authData.token)
      .then((profile) => {
        if (!active) {
          return;
        }

        setProfileName(profile.name);
        setProfileAge(String(profile.age));
      })
      .catch((error) => {
        if (active) {
          setProfileError(getErrorText(error));
        }
      });

    return () => {
      active = false;
    };
  }, [authData?.token]);

  async function onUpdateProfile(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!authData?.token) {
      setProfileError("Please log in first.");
      return;
    }

    try {
      setProfileLoading(true);
      setProfileError(null);
      setProfileMessage(null);
      await api.updateProfile(authData.token, {
        name: profileName,
        age: Number(profileAge) || 0,
      });
      updateAuthName(profileName);
      setProfileMessage("Profile updated.");
    } catch (error) {
      setProfileError(getErrorText(error));
    } finally {
      setProfileLoading(false);
    }
  }

  return (
    <ProtectedRoute>
      <section className="route-frame profile-grid">
        <div className="panel hero-card">
          <span className="tag">Private profile</span>
          <h1 className="page-title">Manage your account</h1>
          <p className="page-copy">
            Keep your public name current so your posts and comments are shown consistently across DevConnect.
          </p>
        </div>

        <div className="panel card">
          <ProfileCard
            profileName={profileName}
            profileAge={profileAge}
            profileLoading={profileLoading}
            profileError={profileError}
            profileMessage={profileMessage}
            onProfileNameChange={setProfileName}
            onProfileAgeChange={setProfileAge}
            onLoadProfile={() => void fetchProfile()}
            onSubmit={onUpdateProfile}
          />
        </div>
      </section>
    </ProtectedRoute>
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