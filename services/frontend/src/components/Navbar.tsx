import { Link, useLocation } from 'react-router-dom';

import { useAuth } from '../hooks/useAuth';
import styles from '../styles/Navbar.module.css';

export function Navbar() {
  const location = useLocation();
  const { isAuthenticated, logout } = useAuth();

  return (
    <header className={styles.navbar}>
      <div className={styles.brand}>Feane</div>
      <nav className={styles.links}>
        {!isAuthenticated && (
          <>
            <Link className={location.pathname === '/login' ? styles.active : ''} to="/login">
              Вход
            </Link>
            <Link className={location.pathname === '/register' ? styles.active : ''} to="/register">
              Регистрация
            </Link>
          </>
        )}
        {isAuthenticated && (
          <>
            <Link className={location.pathname === '/profile' ? styles.active : ''} to="/profile">
              Профиль
            </Link>
            <button className={styles.logout} onClick={logout} type="button">
              Выйти
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
