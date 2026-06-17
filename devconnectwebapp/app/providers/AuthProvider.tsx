"use client";

import { createContext, useContext, useMemo, useState } from "react";
import { api } from "@/lib/api";
import { clearStoredAuth, getStoredAuth, saveStoredAuth } from "@/lib/auth";
import { AuthResponse, LoginRequest, RegisterRequest } from "@/lib/types";

type AuthContextValue = {
  authData: AuthResponse | null;
  isAuthenticated: boolean;
  login: (payload: LoginRequest) => Promise<AuthResponse>;
  register: (payload: RegisterRequest) => Promise<AuthResponse>;
  logout: () => void;
  updateAuthName: (name: string) => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [authData, setAuthData] = useState<AuthResponse | null>(() => getStoredAuth());

  const value = useMemo<AuthContextValue>(
    () => ({
      authData,
      isAuthenticated: Boolean(authData?.token),
      async login(payload) {
        const response = await api.login(payload);
        setAuthData(response);
        saveStoredAuth(response);
        return response;
      },
      async register(payload) {
        const response = await api.register(payload);
        setAuthData(response);
        saveStoredAuth(response);
        return response;
      },
      logout() {
        setAuthData(null);
        clearStoredAuth();
      },
      updateAuthName(name) {
        setAuthData((current) => {
          if (!current) {
            return current;
          }

          const updated = { ...current, name };
          saveStoredAuth(updated);
          return updated;
        });
      },
    }),
    [authData]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}