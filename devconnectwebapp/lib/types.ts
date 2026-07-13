export type SortBy = "createdAt" | "title" | "likes";
export type SortDirection = "asc" | "desc";

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  role: string;
}

export interface ProfileResponse {
  id: number;
  name: string;
  email: string;
  role: string;
  age: number;
  createdAt: string;
}

export interface UpdateProfileRequest {
  name: string;
  age: number;
}

export interface CreatePostRequest {
  title: string;
  content: string;
}

export interface PostResponse {
  id: number;
  title: string;
  content: string;
  createdAt: string;
  authorName: string;
  userId: number;
  likesCount: number;
  commentsCount: number;
  updatedAt?: string;
}

export interface CreateCommentRequest {
  content: string;
}

export interface CommentResponse {
  id: number;
  content: string;
  authorName: string;
  postId: number;
  createdAt: string;
  updatedAt?: string;
}

export interface LikeResponse {
  totalLikes: number;
  likedByMe: boolean;
}

export type BookmarkSortBy = "createdAt" | "title";

export interface BookmarkResponse {
  bookmarked: boolean;
  postId: number;
}

export interface BookmarkStats {
  postId: number;
  title: string;
  bookmarkCount: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}