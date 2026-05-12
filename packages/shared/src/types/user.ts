export type SubscriptionTier = 'free' | 'pro' | 'enterprise';
export type UserRole = 'user' | 'admin';

export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  subscriptionTier: SubscriptionTier;
  role: UserRole;
  createdAt: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}
