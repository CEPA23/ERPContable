import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { deleteEmpresa, getEmpresas } from '../api/empresas';
import { useAppSession } from '../context/AppSessionContext';
import type { Empresa } from '../types/empresa';

function formatDate(value: string) { return new Intl.DateTimeFormat('es-PE', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)); }

export function EmpresasPage() {
  const { empresa: empresaActiva, ejercicio, periodo } = useAppSession();
  const [items, setItems] = useState<Empresa[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [reloadToken, setReloadToken] = useState(0);

  const load = async () => {
    setLoading(true); setError(null);
    try { setItems(await getEmpresas()); } catch (err) { setError(err instanceof Error ? err.message : 'No se pudieron cargar las empresas.'); } finally { setLoading(false); }
  };
  useEffect(() => { void load(); }, [reloadToken]);

  const filteredItems = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('es-PE');
    return items.filter(item => {
      const searchable = `${item.razonSocial} ${item.nombreComercial ?? ''} ${item.documentoIdentidad}`.toLocaleLowerCase('es-PE');
      return (!normalized || searchable.includes(normalized)) && (!statusFilter || (statusFilter === 'ACTIVA' ? item.activa : !item.activa));
    });
  }, [items, search, statusFilter]);
  const hasFilters = Boolean(search || statusFilter);
  const clearFilters = () => { setSearch(''); setStatusFilter(''); };

  const handleDelete = async (id: number) => {
    if (!confirm('¿Deseas eliminar esta empresa?')) return;
    setBusyId(id); setError(null); setMessage(null);
    try { await deleteEmpresa(id); setItems(current => current.filter(item => item.id !== id)); setMessage('Empresa eliminada.'); }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo eliminar la empresa.'); }
    finally { setBusyId(null); }
  };

  return <div className="page-stack admin-page">
    <div className="page-header admin-page-header"><div><span className="eyebrow">Administración contable · Entidades</span><h1>Empresas</h1><p>Administra las entidades contables que operan dentro del sistema.</p></div><div className="admin-header-side"><div className="transaction-context"><span>Contexto actual</span><strong>{empresaActiva?.razonSocial ?? 'Sin empresa seleccionada'}</strong><span className="transaction-company">Ejercicio {ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'Período no seleccionado'}</span></div><Link className="primary-button" to="/empresas/nueva">Nueva empresa</Link></div></div>
    {error ? <div className="error-box" role="alert">{error}<button className="text-button" type="button" onClick={() => setReloadToken(value => value + 1)}>Reintentar</button></div> : null}
    {message ? <div className="success-box" role="status">{message}</div> : null}
    <section className="panel table-panel admin-list-panel"><div className="transaction-list-head"><div><span className="eyebrow">Maestro de entidades</span><h2>Empresas registradas</h2><p className="form-hint">{hasFilters ? `${filteredItems.length} de ${items.length} empresas visibles` : `${items.length} empresas cargadas`}</p></div><div className="admin-list-summary"><span>Estado del maestro</span><strong>{items.filter(item => item.activa).length} activas</strong><small>{items.filter(item => !item.activa).length} inactivas</small></div></div><div className="transaction-filters admin-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="RUC, razón social o nombre comercial" /></label><label><span>Estado</span><select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}><option value="">Todos</option><option value="ACTIVA">Activas</option><option value="INACTIVA">Inactivas</option></select></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>
      {loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando empresas</strong><span>Consultando las entidades contables registradas.</span></div> : items.length === 0 ? <div className="data-state"><strong>No hay empresas registradas todavía</strong><span>Crea la primera entidad contable para comenzar.</span><Link className="secondary-button" to="/empresas/nueva">Crear primera empresa</Link></div> : filteredItems.length === 0 ? <div className="data-state"><strong>No hay resultados para estos filtros</strong><span>Prueba con otro RUC, razón social o estado.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="admin-table company-table"><thead><tr><th>Razón social</th><th>RUC / documento</th><th>Contacto</th><th>Información fiscal</th><th>Estado</th><th>Creada</th><th><span className="sr-only">Acciones</span></th></tr></thead><tbody>{filteredItems.map(item => { const isCurrent = empresaActiva?.id === item.id; return <tr key={item.id} className={isCurrent ? 'current-entity-row' : ''}><td data-label="Razón social"><strong>{item.razonSocial}</strong><small>{item.nombreComercial ?? 'Sin nombre comercial'}</small>{isCurrent ? <span className="current-entity-label">Empresa actual</span> : null}</td><td data-label="RUC / documento">{item.documentoIdentidad}</td><td data-label="Contacto"><strong>{item.email ?? 'Sin email'}</strong><small>{item.telefono ?? 'Sin teléfono'}</small><small>{item.direccion ?? 'Sin dirección'}</small></td><td data-label="Información fiscal">{item.estadoSunat || item.condicionSunat ? <><span>{item.estadoSunat ?? 'Estado no informado'}</span><small>{item.condicionSunat ?? 'Condición no informada'}</small></> : <span className="muted-cell">Sin datos fiscales</span>}</td><td data-label="Estado"><span className={`badge ${item.activa ? 'success' : 'muted'}`}>{item.activa ? 'Activa' : 'Inactiva'}</span></td><td data-label="Creada" className="date-cell">{formatDate(item.creadaEnUtc)}</td><td data-label="Acciones"><div className="row-actions"><Link className="text-button" to={`/empresas/${item.id}/editar`}>Editar</Link><button className="text-button danger" type="button" onClick={() => void handleDelete(item.id)} disabled={busyId === item.id}>{busyId === item.id ? 'Eliminando...' : 'Eliminar'}</button></div></td></tr>; })}</tbody></table></div>}
    </section>
  </div>;
}
