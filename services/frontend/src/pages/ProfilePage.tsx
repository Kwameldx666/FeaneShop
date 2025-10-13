import { useEffect } from 'react';

import { Alert } from '../components/Alert';
import { AuthCard } from '../components/AuthCard';
import { LoadingIndicator } from '../components/LoadingIndicator';
import { SubmitButton } from '../components/SubmitButton';
import { useAuth } from '../hooks/useAuth';

export function ProfilePage() {
  const { user, loading, error, refreshProfile, clearError } = useAuth();

  useEffect(() => {
    clearError();
  }, [clearError]);

  return (
    <AuthCard
      title={user ? user.fullName ?? user.email : 'Профиль'}
      subtitle="Здесь вы можете проверить данные учетной записи"
    >
      {error && <Alert tone="error">{error}</Alert>}
      {loading && <LoadingIndicator />}
      {user ? (
        <div>
          <p>
            <strong>Email:</strong> {user.email}
          </p>
          {user.fullName && (
            <p>
              <strong>Имя:</strong> {user.fullName}
            </p>
          )}
          {user.roles && user.roles.length > 0 && (
            <p>
              <strong>Роли:</strong> {user.roles.join(', ')}
            </p>
          )}
          <SubmitButton disabled={loading} onClick={refreshProfile} type="button">
            Обновить данные
          </SubmitButton>
        </div>
      ) : (
        <Alert tone="info">Данные пользователя недоступны</Alert>
      )}
    </AuthCard>
  );
}
