import {
  AuthResponse,
  CommentResponse,
  CreateCommentRequest,
  CreatePostRequest,
  LikeResponse,
  LoginRequest,
  PagedResult,
  PostResponse,
  ProfileResponse,
  RegisterRequest,
  SortBy,
  SortDirection,
  UpdateProfileRequest,
} from "@/lib/types";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  "https://localhost:7238";

const API_BASE_URL_FALLBACKS =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "")
    ? [API_BASE_URL]
    : ["https://localhost:7238", "http://localhost:5029"];

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
    this.name = "ApiError";
  }
}

type RequestOptions = {
  token?: string;
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
};

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  let lastError: unknown;

  for (const baseUrl of API_BASE_URL_FALLBACKS) {
    try {
      const response = await fetch(`${baseUrl}${path}`, {
        method: options.method ?? "GET",
        headers: {
          "Content-Type": "application/json",
          ...(options.token ? { Authorization: `Bearer ${options.token}` } : {}),
        },
        body: options.body === undefined ? undefined : JSON.stringify(options.body),
      });

      if (!response.ok) {
        const message = await extractError(response);
        throw new ApiError(message, response.status);
      }

      if (response.status === 204) {
        return undefined as T;
      }

      return (await response.json()) as T;
    } catch (error) {
      lastError = error;

      if (error instanceof ApiError || baseUrl === API_BASE_URL_FALLBACKS.at(-1)) {
        throw error;
      }
    }
  }

  throw lastError instanceof Error ? lastError : new Error("Request failed.");
}

async function extractError(response: Response): Promise<string> {
  const contentType = response.headers.get("content-type")?.toLowerCase() ?? "";
  if (contentType.includes("application/json")) {
    const payload = (await response.json()) as
      | { title?: string; message?: string; error?: string }
      | string;
    if (typeof payload === "string") {
      return payload;
    }
    return payload.message ?? payload.error ?? payload.title ?? "Request failed.";
  }

  const text = await response.text();
  return text || "Request failed.";
}

export const api = {
  register(payload: RegisterRequest) {
    return request<AuthResponse>("/api/auth/register", {
      method: "POST",
      body: payload,
    });
  },

  login(payload: LoginRequest) {
    return request<AuthResponse>("/api/auth/login", {
      method: "POST",
      body: payload,
    });
  },

  getProfile(token: string) {
    return request<ProfileResponse>("/api/users/profile", { token });
  },

  updateProfile(token: string, payload: UpdateProfileRequest) {
    return request<void>("/api/users/profile", {
      token,
      method: "PUT",
      body: payload,
    });
  },

  getPosts(query: {
    pageNumber: number;
    pageSize: number;
    sortBy: SortBy;
    sortDirection: SortDirection;
  }) {
    const params = new URLSearchParams({
      pageNumber: String(query.pageNumber),
      pageSize: String(query.pageSize),
      sortBy: query.sortBy,
      sortDirection: query.sortDirection,
    });
    return request<PagedResult<PostResponse>>(`/api/posts?${params.toString()}`);
  },

  getPostById(postId: number) {
    return request<PostResponse>(`/api/posts/${postId}`);
  },

  createPost(token: string, payload: CreatePostRequest) {
    return request<PostResponse>("/api/posts", {
      token,
      method: "POST",
      body: payload,
    });
  },

  getLikes(postId: number, token?: string) {
    return request<LikeResponse>(`/api/posts/${postId}/likes`, { token });
  },

  toggleLike(postId: number, token: string) {
    return request<LikeResponse>(`/api/posts/${postId}/likes`, {
      token,
      method: "POST",
    });
  },

  getComments(postId: number) {
    return request<CommentResponse[]>(`/api/posts/${postId}/comments`);
  },

  addComment(postId: number, token: string, payload: CreateCommentRequest) {
    return request<CommentResponse>(`/api/posts/${postId}/comments`, {
      token,
      method: "POST",
      body: payload,
    });
  },
};

export function getApiBaseUrl(): string {
  return API_BASE_URL;
}