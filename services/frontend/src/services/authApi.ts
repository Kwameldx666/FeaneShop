import { AuthenticatedUser, LoginPayload, RegisterPayload } from '../types';

const gatewayBaseUrl = (import.meta.env.VITE_GATEWAY_BASE_URL as string | undefined) ?? 'http://localhost:5200';
const authPrefix = (import.meta.env.VITE_AUTH_PREFIX as string | undefined) ?? '/api/auth';

interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
}

interface RegisterResponse {
  userId: string;
  email: string;
}

export async function login(payload: LoginPayload): Promise<LoginResponse> {
  const response = await fetch(`${gatewayBaseUrl}${authPrefix}/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const error = await safeReadError(response);
    throw new Error(error ?? 'Не удалось выполнить вход');
  }

  return response.json();
}

export async function register(payload: RegisterPayload): Promise<RegisterResponse> {
  const response = await fetch(`${gatewayBaseUrl}${authPrefix}/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const error = await safeReadError(response);
    throw new Error(error ?? 'Не удалось завершить регистрацию');
  }

  return response.json();
}

export async function fetchProfile(token: string): Promise<AuthenticatedUser> {
  const response = await fetch(`${gatewayBaseUrl}${authPrefix}/profile`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    const error = await safeReadError(response);
    throw new Error(error ?? 'Не удалось получить данные профиля');
  }

  return response.json();
}

async function safeReadError(response: Response) {
  try {
    const data = await response.json();
    if (typeof data === 'string') {
      return data;
    }
    if (data && typeof data.message === 'string') {
      return data.message;
    }
    if (Array.isArray(data?.errors)) {
      return data.errors.join(', ');
    }
    return undefined;
  } catch (error) {
    console.error('Failed to parse error response', error);
    return undefined;
  }
}
