import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { restablecerContrasena } from '../api/auth';

export function RestablecerContrasenaPage() {
  const [params] = useSearchParams(); const [correo, setCorreo] = useState(params.get('email') ?? ''); const [contrasena, setContrasena] = useState(''); const [message, setMessage] = useState<string | null>(null); const [error, setError] = useState<string | null>(null);
  const submit = async (event: React.FormEvent) => { event.preventDefault(); setError(null); try { await restablecerContrasena(correo, params.get('token') ?? '', contrasena); setMessage('Contraseña actualizada. Ya puedes iniciar sesión.'); } catch (err) { setError(err instanceof Error ? err.message : 'No se pudo restablecer la contraseña.'); } };
  return <div className="auth-shell"><form className="auth-card panel" onSubmit={submit}><div className="brand-mark">EC</div><span className="eyebrow">Seguridad</span><h1>Nueva contraseña</h1>{error ? <div className="error-box">{error}</div> : null}{message ? <div className="success-box">{message}</div> : null}<label><span>Correo electrónico</span><input type="email" value={correo} onChange={(event) => setCorreo(event.target.value)} required /></label><label><span>Nueva contraseña</span><input type="password" value={contrasena} onChange={(event) => setContrasena(event.target.value)} minLength={10} required /></label><button className="primary-button">Actualizar contraseña</button><div className="auth-links"><Link to="/login">Ir al inicio de sesión</Link></div></form></div>;
}
