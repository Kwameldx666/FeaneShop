import { ButtonHTMLAttributes } from 'react';

import styles from '../styles/SubmitButton.module.css';

export function SubmitButton({ children, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button className={styles.button} type="submit" {...props}>
      {children}
    </button>
  );
}
