import { InputHTMLAttributes, ReactNode } from 'react';

import styles from '../styles/FormField.module.css';

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  hint?: ReactNode;
  error?: string;
}

export function FormField({ label, hint, error, ...inputProps }: FormFieldProps) {
  const fieldId = inputProps.id ?? inputProps.name;

  return (
    <label className={styles.field} htmlFor={fieldId}>
      <span className={styles.label}>{label}</span>
      <input className={styles.input} id={fieldId} {...inputProps} />
      {hint && !error && <span className={styles.hint}>{hint}</span>}
      {error && <span className={styles.error}>{error}</span>}
    </label>
  );
}
