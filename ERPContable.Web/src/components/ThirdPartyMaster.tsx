import { useMemo, useState } from 'react';

export type ThirdPartyItem = {
  id: number;
  documento: string;
  razonSocial: string;
  activo: boolean;
  direccion?: string;
  email?: string;
};

type ThirdPartyMasterProps = {
  kind: 'clientes' | 'proveedores';
  companyName?: string;
  ejercicio?: number | null;
  periodo?: number | null;
  items: ThirdPartyItem[];
  loading: boolean;
  error: string | null;
  saving: boolean;
  message: string | null;
  messageKind: 'success' | 'error';
  onRetry: () => void;
  onCreate: (payload: { documento: string; razonSocial: string }) => Promise<boolean>;
};

export function ThirdPartyMaster({ kind, companyName, ejercicio, periodo, items, loading, error, saving, message, messageKind, onRetry, onCreate }: ThirdPartyMasterProps) {
  const [documento, setDocumento] = useState('');
  const [razonSocial, setRazonSocial] = useState('');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const label = kind === 'clientes' ? 'Cliente' : 'Proveedor';
  const labelPlural = kind === 'clientes' ? 'Clientes' : 'Proveedores';
  const normalizedSearch = search.trim().toLocaleLowerCase('es-PE');
  const filteredItems = useMemo(() => items.filter(item => {
    const searchable = `${item.documento} ${item.razonSocial}`.toLocaleLowerCase('es-PE');
    return (!normalizedSearch || searchable.includes(normalizedSearch)) && (!statusFilter || (statusFilter === 'ACTIVO' ? item.activo : !item.activo));
  }), [items, normalizedSearch, statusFilter]);
  const hasFilters = Boolean(search || statusFilter);
  const clearFilters = () => { setSearch(''); setStatusFilter(''); };
  const submit = async () => {
    const created = await onCreate({ documento, razonSocial });
    if (created) { setDocumento(''); setRazonSocial(''); }
  };

  return <div className="page-stack master-page">
    <div className="page-header master-page-header"><div><span className="eyebrow">Maestros contables · {labelPlural}</span><h1>{labelPlural}</h1><p>Administra los terceros utilizados en las operaciones de {kind === 'clientes' ? 'venta' : 'compra'}.</p></div><div className="transaction-context"><span>Empresa activa</span><strong>{companyName}</strong><span className="transaction-company">Ejercicio {ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'Período no seleccionado'}</span></div></div>

    <section className="panel form-panel master-form" id="nuevo-tercero"><div className="transaction-form-head"><div><span className="eyebrow">Identificación</span><h2>Nuevo {label.toLowerCase()}</h2><p className="form-hint">Registra el tercero que será seleccionado en los comprobantes.</p></div><a className="secondary-button" href="#maestro-listado">Ir al listado</a></div><div className="master-form-grid"><label><span>RUC / documento</span><input value={documento} onChange={e => setDocumento(e.target.value)} placeholder="20123456789" /></label><label><span>Razón social / nombre</span><input value={razonSocial} onChange={e => setRazonSocial(e.target.value)} placeholder={kind === 'clientes' ? 'Cliente SAC' : 'Proveedor SAC'} /></label></div>{message ? <div className={messageKind === 'success' ? 'success-box' : 'error-box'} role="status">{message}</div> : null}<div className="form-actions"><button className="primary-button" type="button" onClick={() => void submit()} disabled={saving}>{saving ? 'Guardando...' : `Registrar ${label.toLowerCase()}`}</button></div></section>

    <section className="panel table-panel master-list-panel" id="maestro-listado"><div className="transaction-list-head"><div><span className="eyebrow">Consulta del maestro</span><h2>{labelPlural} registrados</h2><p className="form-hint">{hasFilters ? `${filteredItems.length} de ${items.length} registros visibles` : `${items.length} registros cargados`}</p></div><div className="master-list-summary"><span>Estado del maestro</span><strong>{items.filter(item => item.activo).length} activos</strong><small>{items.filter(item => !item.activo).length} inactivos</small></div></div><div className="transaction-filters master-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Documento o razón social" /></label><label><span>Estado</span><select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}><option value="">Todos</option><option value="ACTIVO">Activos</option><option value="INACTIVO">Inactivos</option></select></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>{error ? <div className="data-state data-state-error"><strong>No se pudo cargar el maestro</strong><span>{error}</span><button className="secondary-button" type="button" onClick={onRetry}>Reintentar</button></div> : loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando {labelPlural.toLowerCase()}</strong><span>Consultando los terceros de la empresa activa.</span></div> : items.length === 0 ? <div className="data-state"><strong>Aún no hay {labelPlural.toLowerCase()} registrados</strong><span>Los terceros creados aparecerán aquí para su selección en comprobantes.</span></div> : filteredItems.length === 0 ? <div className="data-state"><strong>No hay resultados para estos filtros</strong><span>Prueba con otro documento, nombre o estado.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="master-table"><thead><tr><th>Documento</th><th>Razón social / nombre</th><th>Contacto disponible</th><th>Estado</th></tr></thead><tbody>{filteredItems.map(item => <tr key={item.id}><td data-label="Documento"><strong>{item.documento}</strong><small>Registro #{item.id}</small></td><td data-label="Razón social / nombre">{item.razonSocial}</td><td data-label="Contacto disponible">{item.email ?? item.direccion ?? <span className="muted-cell">No disponible</span>}</td><td data-label="Estado"><span className={`badge ${item.activo ? 'success' : 'muted'}`}>{item.activo ? 'Activo' : 'Inactivo'}</span></td></tr>)}</tbody></table></div>}</section>
  </div>;
}
