import { useEffect, useState } from 'react';

export type SystemStatus = 'loading' | 'ok' | 'degraded' | 'offline';

export function useSystemHealth() {
  const [status, setStatus] = useState<SystemStatus>('loading');

  useEffect(() => {
    let active = true;
    const check = async () => {
      if (!navigator.onLine) {
        if (active) setStatus('offline');
        return;
      }
      try {
        const response = await fetch('/api/system/health', { cache: 'no-store' });
        if (active) setStatus(response.ok ? 'ok' : 'degraded');
      } catch {
        if (active) setStatus('offline');
      }
    };
    const checkNow = () => void check();
    void check();
    window.addEventListener('online', checkNow);
    window.addEventListener('offline', checkNow);
    const timer = window.setInterval(check, 10000);
    return () => {
      active = false;
      window.clearInterval(timer);
      window.removeEventListener('online', checkNow);
      window.removeEventListener('offline', checkNow);
    };
  }, []);

  return status;
}

export function systemStatusLabel(status: SystemStatus) {
  return status === 'ok' ? 'Operativo' : status === 'degraded' ? 'Con problemas' : status === 'offline' ? 'Sin conexión' : 'Verificando...';
}
