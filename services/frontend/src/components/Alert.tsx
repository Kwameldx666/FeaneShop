import { PropsWithChildren } from 'react';

import styles from '../styles/Alert.module.css';

type AlertTone = 'success' | 'error' | 'info';

interface AlertProps extends PropsWithChildren {
  tone?: AlertTone;
}

export function Alert({ tone = 'info', children }: AlertProps) {
  return <div className={`${styles.alert} ${styles[tone]}`}>{children}</div>;
}
