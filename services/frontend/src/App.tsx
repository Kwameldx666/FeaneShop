import { Navigate, Route, Routes } from 'react-router-dom';

import { Navbar } from './components/Navbar';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useAuth } from './hooks/useAuth';
import { LoginPage } from './pages/LoginPage';
import { ProfilePage } from './pages/ProfilePage';
import { RegisterPage } from './pages/RegisterPage';
import styles from './styles/App.module.css';

function App() {
  const { isAuthenticated } = useAuth();

  return (
    <div className={styles.container}>
      <Navbar />
      <main className={styles.main}>
        <Routes>
          <Route path="/" element={<Navigate to={isAuthenticated ? '/profile' : '/login'} replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/profile"
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
      <footer className={styles.footer}>
        <p>Feane • Авторизация и управление аккаунтом</p>
      </footer>
    </div>
  );
}

export default App;
