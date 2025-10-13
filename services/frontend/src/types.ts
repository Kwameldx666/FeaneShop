export interface AuthTokens {
  accessToken: string;
  refreshToken?: string;
}

export interface AuthenticatedUser {
  id: string;
  email: string;
  fullName?: string;
  roles?: string[];
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  email: string;
  password: string;
  fullName?: string;
}
