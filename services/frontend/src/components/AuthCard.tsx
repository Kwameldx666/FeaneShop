import { PropsWithChildren } from 'react';

import styles from '../styles/AuthCard.module.css';

interface AuthCardProps extends PropsWithChildren {
  title: string;
  subtitle?: string;
}

export function AuthCard({ title, subtitle, children }: AuthCardProps) {
  return (
    <section className={styles.card}>
      <header>
        <h1>{title}</h1>
        {subtitle && <p>{subtitle}</p>}
      </header>
      <div>{children}</div>
    </section>
  );
}
