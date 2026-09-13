import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSession } from '../context/AppSessionContext';

export function CambiarContrasenaPage() {
  const navigate = useNavigate(); const { cambiarContrasena } = useAppSession();
  const [actual, setActual] = useState(''); const [nueva, setNueva] = useState(''); const [error, setError] = useState<string | null>(null); const [busy, setBusy] = useState(false);
  const submit = async (event: React.FormEvent) => { event.preventDefault(); setBusy(true); setError(null); try { await cambiarContrasena(actual, nueva); navigate('/login', { replace: true, state: { message: 'Contraseña actualizada. Ingresa nuevamente.' } }); } catch (err) { setError(err instanceof Error ? err.message : 'No se pudo actualizar la contraseña.'); } finally { setBusy(false); } };
  return <div className="page-stack"><div className="page-header"><div><span className="eyebrow">Seguridad</span><h1>Cambiar contraseña</h1><p>Al actualizarla, se cerrarán todas las sesiones activas.</p></div></div><form className="panel form-panel form-grid" onSubmit={submit}>{error ? <div className="error-box span-2">{error}</div> : null}<label><span>Contraseña actual</span><input type="password" autoComplete="current-password" value={actual} onChange={(event) => setActual(event.target.value)} required /></label><label><span>Nueva contraseña</span><input type="password" autoComplete="new-password" value={nueva} onChange={(event) => setNueva(event.target.value)} minLength={10} required /></label><div className="form-actions span-2"><button className="primary-button" disabled={busy}>{busy ? 'Actualizando...' : 'Actualizar contraseña'}</button></div></form></div>;
}
