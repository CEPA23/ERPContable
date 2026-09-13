import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppSession } from '../context/AppSessionContext';

export function LoginPage() {
  const navigate = useNavigate();
  const { login, selectEmpresa } = useAppSession();
  const [correo, setCorreo] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    setBusy(true); setError(null);
    try {
      let session = await login(correo, contrasena);
      if (session.empresas.length === 1 && !session.empresaActiva) session = await selectEmpresa(session.empresas[0].empresa);
      navigate(session.empresas.length === 0 ? '/onboarding/empresa' : session.empresaActiva ? '/' : '/seleccion/empresa', { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'No se pudo iniciar sesión.');
    } finally { setBusy(false); }
  };

  return <div className="auth-shell"><form className="auth-card panel" onSubmit={submit}>
    <div className="brand-mark">EC</div><span className="eyebrow">ERP Contable</span><h1>Iniciar sesión</h1><p>Accede con tu correo y contraseña.</p>
    {error ? <div className="error-box">{error}</div> : null}
    <label><span>Correo electrónico</span><input type="email" autoComplete="email" value={correo} onChange={(event) => setCorreo(event.target.value)} required /></label>
    <label><span>Contraseña</span><input type="password" autoComplete="current-password" value={contrasena} onChange={(event) => setContrasena(event.target.value)} required /></label>
    <button className="primary-button" type="submit" disabled={busy}>{busy ? 'Ingresando...' : 'Ingresar'}</button>
    <div className="auth-links"><Link to="/registro">Crear cuenta</Link><Link to="/recuperar-contrasena">¿Olvidaste tu contraseña?</Link></div>
  </form></div>;
}
