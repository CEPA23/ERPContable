import { useState } from 'react';
import { Link } from 'react-router-dom';
import { solicitarRecuperacion } from '../api/auth';

export function RecuperarContrasenaPage() {
  const [correo, setCorreo] = useState(''); const [message, setMessage] = useState<string | null>(null); const [busy, setBusy] = useState(false);
  const submit = async (event: React.FormEvent) => { event.preventDefault(); setBusy(true); try { await solicitarRecuperacion(correo); setMessage('Si el correo existe, recibirás las instrucciones para restablecer la contraseña.'); } finally { setBusy(false); } };
  return <div className="auth-shell"><form className="auth-card panel" onSubmit={submit}><div className="brand-mark">EC</div><span className="eyebrow">Seguridad</span><h1>Recuperar contraseña</h1><p>Te enviaremos un enlace seguro al correo registrado.</p>{message ? <div className="success-box">{message}</div> : null}<label><span>Correo electrónico</span><input type="email" value={correo} onChange={(event) => setCorreo(event.target.value)} required /></label><button className="primary-button" disabled={busy}>{busy ? 'Enviando...' : 'Enviar instrucciones'}</button><div className="auth-links"><Link to="/login">Volver al inicio de sesión</Link></div></form></div>;
}
