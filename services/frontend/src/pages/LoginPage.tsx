import { FormEvent, useState } from 'react';
import { Link, useLocation, useNavigate, type Location } from 'react-router-dom';

import { Alert } from '../components/Alert';
import { AuthCard } from '../components/AuthCard';
import { FormField } from '../components/FormField';
import { LoadingIndicator } from '../components/LoadingIndicator';
import { SubmitButton } from '../components/SubmitButton';
import { useAuth } from '../hooks/useAuth';

interface LocationState {
  from?: Location;
}

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as LocationState | undefined)?.from;
  const { login, loading, error, clearError } = useAuth();
  const [form, setForm] = useState({ email: '', password: '' });
  const [localError, setLocalError] = useState<string | undefined>();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLocalError(undefined);
    clearError();

    if (!form.email || !form.password) {
      setLocalError('Введите email и пароль');
      return;
    }

    try {
      await login(form);
      navigate(from?.pathname ?? '/profile', { replace: true });
    } catch (err) {
      console.warn('Login failed', err);
    }
  }

  return (
    <AuthCard title="Добро пожаловать" subtitle="Войдите, чтобы управлять своими заказами и бронированиями">
      {(error || localError) && <Alert tone="error">{error ?? localError}</Alert>}
      {loading && <LoadingIndicator />}
      <form onSubmit={handleSubmit} noValidate>
        <FormField
          name="email"
          type="email"
          autoComplete="email"
          label="Email"
          value={form.email}
          onChange={(event) => setForm((prev) => ({ ...prev, email: event.target.value }))}
          placeholder="you@example.com"
        />
        <FormField
          name="password"
          type="password"
          autoComplete="current-password"
          label="Пароль"
          value={form.password}
          onChange={(event) => setForm((prev) => ({ ...prev, password: event.target.value }))}
        />
        <SubmitButton disabled={loading}>Войти</SubmitButton>
      </form>
      <p>
        Нет аккаунта?{' '}
        <Link to="/register" replace>
          Создать
        </Link>
      </p>
    </AuthCard>
  );
}
