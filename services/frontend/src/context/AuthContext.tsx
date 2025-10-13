import { PropsWithChildren, createContext, useCallback, useEffect, useMemo, useState } from 'react';

import { fetchProfile, login as loginRequest, register as registerRequest } from '../services/authApi';
import { AuthenticatedUser, LoginPayload, RegisterPayload } from '../types';

interface AuthContextValue {
  user?: AuthenticatedUser;
  token?: string;
  isAuthenticated: boolean;
  loading: boolean;
  error?: string;
  login: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
  clearError: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
const TOKEN_STORAGE_KEY = 'feane:auth-token';

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | undefined>();
  const [user, setUser] = useState<AuthenticatedUser | undefined>();
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    const persistedToken = window.localStorage.getItem(TOKEN_STORAGE_KEY);
    if (persistedToken) {
      setToken(persistedToken);
    }
  }, []);

  useEffect(() => {
    if (!token) {
      setUser(undefined);
      return;
    }

    let cancelled = false;

    async function bootstrap() {
      setLoading(true);
      try {
        const profile = await fetchProfile(token);
        if (!cancelled) {
          setUser(profile);
        }
      } catch (err) {
        console.warn('Не удалось получить профиль пользователя', err);
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Ошибка загрузки профиля');
          setToken(undefined);
          window.localStorage.removeItem(TOKEN_STORAGE_KEY);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    bootstrap();

    return () => {
      cancelled = true;
    };
  }, [token]);

  const handleLogin = useCallback(async (payload: LoginPayload) => {
    setLoading(true);
    setError(undefined);
    try {
      const response = await loginRequest(payload);
      setToken(response.accessToken);
      window.localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken);
      const profile = await fetchProfile(response.accessToken);
      setUser(profile);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка входа');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const handleRegister = useCallback(
    async (payload: RegisterPayload) => {
      setLoading(true);
      setError(undefined);
      try {
        await registerRequest(payload);
        await handleLogin({ email: payload.email, password: payload.password });
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Ошибка регистрации');
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [handleLogin]
  );

  const handleLogout = useCallback(() => {
    setToken(undefined);
    setUser(undefined);
    window.localStorage.removeItem(TOKEN_STORAGE_KEY);
  }, []);

  const refreshProfile = useCallback(async () => {
    if (!token) {
      return;
    }
    setLoading(true);
    setError(undefined);
    try {
      const profile = await fetchProfile(token);
      setUser(profile);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось обновить профиль');
    } finally {
      setLoading(false);
    }
  }, [token]);

  const clearError = useCallback(() => setError(undefined), []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token && user),
      loading,
      error,
      login: handleLogin,
      register: handleRegister,
      logout: handleLogout,
      refreshProfile,
      clearError
    }),
    [user, token, loading, error, handleLogin, handleRegister, handleLogout, refreshProfile, clearError]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
