import styles from '../styles/LoadingIndicator.module.css';

export function LoadingIndicator() {
  return (
    <div className={styles.wrapper}>
      <div className={styles.spinner} />
      <span>Загрузка...</span>
    </div>
  );
}
