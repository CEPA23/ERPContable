import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { registrar } from '../api/auth';

export function RegistroPage() {
  const navigate = useNavigate();
  const [nombre, setNombre] = useState(''); const [correo, setCorreo] = useState(''); const [contrasena, setContrasena] = useState('');
  const [message, setMessage] = useState<string | null>(null); const [error, setError] = useState<string | null>(null); const [busy, setBusy] = useState(false);
  const submit = async (event: React.FormEvent) => {
    event.preventDefault(); setBusy(true); setError(null);
    try { const response = await registrar(nombre, correo, contrasena); setMessage(response.mensaje); if (!response.requiereConfirmacionCorreo) window.setTimeout(() => navigate('/login'), 1400); }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo crear la cuenta.'); }
    finally { setBusy(false); }
  };
  return <div className="auth-shell"><form className="auth-card panel" onSubmit={submit}><div className="brand-mark">EC</div><span className="eyebrow">Nueva cuenta</span><h1>Crear cuenta</h1><p>Regístrate con tus datos personales. La empresa se configura después.</p>{error ? <div className="error-box">{error}</div> : null}{message ? <div className="success-box">{message}</div> : null}<label><span>Nombre completo</span><input autoComplete="name" value={nombre} onChange={(event) => setNombre(event.target.value)} required /></label><label><span>Correo electrónico</span><input type="email" autoComplete="email" value={correo} onChange={(event) => setCorreo(event.target.value)} required /></label><label><span>Contraseña</span><input type="password" autoComplete="new-password" value={contrasena} onChange={(event) => setContrasena(event.target.value)} minLength={10} required /></label><p className="form-hint">Mínimo 10 caracteres, con mayúscula, minúscula, número y símbolo.</p><button className="primary-button" type="submit" disabled={busy}>{busy ? 'Creando...' : 'Crear cuenta'}</button><div className="auth-links"><Link to="/login">Volver al inicio de sesión</Link></div></form></div>;
}
