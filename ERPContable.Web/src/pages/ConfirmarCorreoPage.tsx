import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { confirmarCorreo } from '../api/auth';

export function ConfirmarCorreoPage() {
  const [params] = useSearchParams(); const [message, setMessage] = useState('Confirmando correo...');
  useEffect(() => { void confirmarCorreo(params.get('userId') ?? '', params.get('token') ?? '').then(() => setMessage('Correo confirmado. Ya puedes iniciar sesión.')).catch(() => setMessage('El enlace de confirmación no es válido o expiró.')); }, [params]);
  return <div className="auth-shell"><div className="auth-card panel"><div className="brand-mark">EC</div><span className="eyebrow">Cuenta</span><h1>Confirmación de correo</h1><p>{message}</p><div className="auth-links"><Link to="/login">Ir al inicio de sesión</Link></div></div></div>;
}
