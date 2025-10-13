import { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { Alert } from '../components/Alert';
import { AuthCard } from '../components/AuthCard';
import { FormField } from '../components/FormField';
import { LoadingIndicator } from '../components/LoadingIndicator';
import { SubmitButton } from '../components/SubmitButton';
import { useAuth } from '../hooks/useAuth';

export function RegisterPage() {
  const navigate = useNavigate();
  const { register, loading, error, clearError } = useAuth();
  const [form, setForm] = useState({ email: '', password: '', fullName: '' });
  const [localError, setLocalError] = useState<string | undefined>();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearError();
    setLocalError(undefined);

    if (!form.email || !form.password) {
      setLocalError('Email и пароль обязательны');
      return;
    }

    try {
      await register(form);
      navigate('/profile', { replace: true });
    } catch (err) {
      console.warn('Registration failed', err);
    }
  }

  return (
    <AuthCard title="Создать аккаунт" subtitle="Регистрация откроет доступ к персонализированным рекомендациям и заказам">
      {(error || localError) && <Alert tone="error">{error ?? localError}</Alert>}
      {loading && <LoadingIndicator />}
      <form onSubmit={handleSubmit} noValidate>
        <FormField
          name="fullName"
          type="text"
          autoComplete="name"
          label="Имя"
          placeholder="Иван Иванов"
          value={form.fullName}
          onChange={(event) => setForm((prev) => ({ ...prev, fullName: event.target.value }))}
          hint="Укажите как к вам обращаться"
        />
        <FormField
          name="email"
          type="email"
          autoComplete="email"
          label="Email"
          placeholder="you@example.com"
          value={form.email}
          onChange={(event) => setForm((prev) => ({ ...prev, email: event.target.value }))}
        />
        <FormField
          name="password"
          type="password"
          autoComplete="new-password"
          label="Пароль"
          value={form.password}
          onChange={(event) => setForm((prev) => ({ ...prev, password: event.target.value }))}
          hint="Минимум 6 символов"
        />
        <SubmitButton disabled={loading}>Зарегистрироваться</SubmitButton>
      </form>
      <p>
        Уже есть аккаунт?{' '}
        <Link to="/login" replace>
          Войти
        </Link>
      </p>
    </AuthCard>
  );
}
