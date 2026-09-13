import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { type RolEmpresa } from '../api/auth';
import { agregarUsuarioEmpresa, actualizarUsuarioEmpresa, getUsuariosEmpresa, type UsuarioEmpresa } from '../api/usuarios';
import { useAppSession } from '../context/AppSessionContext';

const roles: RolEmpresa[] = ['Administrador', 'Contador', 'Cajero', 'Gerente', 'Auditor'];

export function UsuariosPage() {
  const { empresa, ejercicio, periodo } = useAppSession();
  const [items, setItems] = useState<UsuarioEmpresa[]>([]);
  const [correo, setCorreo] = useState('');
  const [rol, setRol] = useState<RolEmpresa>('Contador');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [busyUserId, setBusyUserId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [reloadToken, setReloadToken] = useState(0);

  const load = async () => {
    if (!empresa) return;
    setLoading(true); setError(null);
    try { setItems(await getUsuariosEmpresa(empresa.id)); } catch (err) { setError(err instanceof Error ? err.message : 'No se pudieron cargar los usuarios.'); } finally { setLoading(false); }
  };
  useEffect(() => { void load(); }, [empresa?.id, reloadToken]);

  const filteredItems = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('es-PE');
    return items.filter(item => {
      const searchable = `${item.nombreCompleto} ${item.correoElectronico}`.toLocaleLowerCase('es-PE');
      return (!normalized || searchable.includes(normalized)) && (!roleFilter || item.rol === roleFilter) && (!statusFilter || (statusFilter === 'ACTIVO' ? item.activo : !item.activo));
    });
  }, [items, roleFilter, search, statusFilter]);
  const hasFilters = Boolean(search || roleFilter || statusFilter);
  const clearFilters = () => { setSearch(''); setRoleFilter(''); setStatusFilter(''); };

  const add = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!empresa) return;
    setSaving(true); setError(null); setMessage(null);
    try { await agregarUsuarioEmpresa(empresa.id, correo, rol); setCorreo(''); setMessage('Usuario agregado a la empresa.'); await load(); }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo agregar el usuario.'); }
    finally { setSaving(false); }
  };

  const update = async (item: UsuarioEmpresa, nextRole: RolEmpresa, active: boolean) => {
    if (!empresa) return;
    setBusyUserId(item.usuarioId); setError(null); setMessage(null);
    try { await actualizarUsuarioEmpresa(empresa.id, item.usuarioId, nextRole, active); setMessage('Usuario actualizado.'); await load(); }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo actualizar el usuario.'); }
    finally { setBusyUserId(null); }
  };

  return <div className="page-stack admin-page users-page">
    <div className="page-header admin-page-header"><div><span className="eyebrow">Administración de acceso</span><h1>Usuarios y roles</h1><p>Administra las personas que utilizan el ERP en {empresa?.razonSocial}.</p></div><div className="transaction-context"><span>Empresa / período</span><strong>{empresa?.razonSocial}</strong><span className="transaction-company">Ejercicio {ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'Período no seleccionado'}</span></div></div>
    {error ? <div className="error-box" role="alert">{error}<button className="text-button" type="button" onClick={() => setReloadToken(value => value + 1)}>Reintentar</button></div> : null}
    {message ? <div className="success-box" role="status">{message}</div> : null}
    <section className="panel form-panel admin-form"><div className="transaction-form-head"><div><span className="eyebrow">Acceso a la empresa</span><h2>Agregar usuario</h2><p className="form-hint">El usuario debe estar registrado para asociarlo a esta empresa.</p></div><span className="transaction-flow-badge">Usuario → Rol</span></div><form className="admin-user-form" onSubmit={add}><label><span>Correo del usuario registrado</span><input type="email" value={correo} onChange={event => setCorreo(event.target.value)} placeholder="usuario@empresa.com" required /></label><label><span>Rol en esta empresa</span><select value={rol} onChange={event => setRol(event.target.value as RolEmpresa)}>{roles.map(option => <option key={option}>{option}</option>)}</select></label><button className="primary-button" type="submit" disabled={saving || loading}>{saving ? 'Agregando...' : 'Agregar usuario'}</button></form></section>
    <section className="panel table-panel admin-list-panel"><div className="transaction-list-head"><div><span className="eyebrow">Consulta de acceso</span><h2>Usuarios asociados</h2><p className="form-hint">{hasFilters ? `${filteredItems.length} de ${items.length} usuarios visibles` : `${items.length} usuarios cargados`}</p></div><div className="admin-list-summary"><span>Estado del acceso</span><strong>{items.filter(item => item.activo).length} activos</strong><small>{items.filter(item => !item.activo).length} inactivos</small></div></div><div className="transaction-filters admin-user-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Nombre o correo" /></label><label><span>Rol</span><select value={roleFilter} onChange={e => setRoleFilter(e.target.value)}><option value="">Todos</option>{roles.map(option => <option key={option}>{option}</option>)}</select></label><label><span>Estado</span><select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}><option value="">Todos</option><option value="ACTIVO">Activos</option><option value="INACTIVO">Inactivos</option></select></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>{loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando usuarios</strong><span>Consultando los accesos asociados a la empresa.</span></div> : items.length === 0 ? <div className="data-state"><strong>No hay usuarios asociados</strong><span>Agrega un usuario registrado para darle acceso a esta empresa.</span></div> : filteredItems.length === 0 ? <div className="data-state"><strong>No hay resultados para estos filtros</strong><span>Prueba con otro nombre, rol o estado.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="admin-table user-table"><thead><tr><th>Usuario</th><th>Correo</th><th>Rol</th><th>Estado</th></tr></thead><tbody>{filteredItems.map(item => <tr key={item.usuarioId}><td data-label="Usuario"><strong>{item.nombreCompleto}</strong><small>Usuario de {empresa?.razonSocial}</small></td><td data-label="Correo">{item.correoElectronico}</td><td data-label="Rol"><select className="admin-role-select" value={item.rol} disabled={busyUserId === item.usuarioId} onChange={event => void update(item, event.target.value as RolEmpresa, item.activo)}>{roles.map(option => <option key={option}>{option}</option>)}</select></td><td data-label="Estado"><button className={`badge admin-status-button ${item.activo ? 'success' : 'muted'}`} type="button" disabled={busyUserId === item.usuarioId} onClick={() => void update(item, item.rol, !item.activo)}>{busyUserId === item.usuarioId ? 'Procesando...' : item.activo ? 'Activo' : 'Inactivo'}</button></td></tr>)}</tbody></table></div>}
    </section>
  </div>;
}
