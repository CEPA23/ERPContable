import { useEffect, useMemo, useState } from 'react';
import { getCuentas, type Cuenta } from '../api/contabilidad';
import { createMovimientoCaja, getMovimientosCaja, type MovimientoCaja } from '../api/caja';
import { useAppSession } from '../context/AppSessionContext';

const today = new Date().toISOString().slice(0, 10);
const money = (value: number) => `S/ ${value.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
const dateLabel = (value: string) => new Date(value).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' });

export function CajaPage() {
  const { empresa, ejercicio, periodo } = useAppSession();
  const empresaId = empresa?.id ?? 0;
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [items, setItems] = useState<MovimientoCaja[]>([]);
  const [tipo, setTipo] = useState('INGRESO');
  const [fecha, setFecha] = useState(today);
  const [descripcion, setDescripcion] = useState('');
  const [monto, setMonto] = useState('');
  const [cuentaCajaId, setCuentaCajaId] = useState(0);
  const [cuentaContrapartidaId, setCuentaContrapartidaId] = useState(0);
  const [message, setMessage] = useState<string | null>(null);
  const [messageKind, setMessageKind] = useState<'success' | 'error'>('success');
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');
  const [tipoFiltro, setTipoFiltro] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    if (!empresaId) return;
    let active = true;
    setLoading(true); setLoadError(null);
    void Promise.all([getCuentas(empresaId), getMovimientosCaja(empresaId)]).then(([c, m]) => {
      if (!active) return;
      setCuentas(c); setItems(m); setCuentaCajaId(c.find(x => x.codigo.startsWith('10'))?.id ?? 0);
    }).catch(e => { if (active) setLoadError(e instanceof Error ? e.message : 'No se pudo cargar caja.'); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [empresaId, reloadToken]);

  const filteredItems = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('es-PE');
    return items.filter(item => {
      const searchable = `${item.descripcion} ${item.cuentaCaja} ${item.cuentaContrapartida} ${item.id}`.toLocaleLowerCase('es-PE');
      const date = item.fecha.slice(0, 10);
      return (!normalized || searchable.includes(normalized)) && (!tipoFiltro || item.tipo === tipoFiltro) && (!dateFrom || date >= dateFrom) && (!dateTo || date <= dateTo);
    });
  }, [dateFrom, dateTo, items, search, tipoFiltro]);

  const totals = useMemo(() => filteredItems.reduce((sum, item) => item.tipo === 'INGRESO' ? { ingreso: sum.ingreso + item.monto, egreso: sum.egreso } : { ingreso: sum.ingreso, egreso: sum.egreso + item.monto }, { ingreso: 0, egreso: 0 }), [filteredItems]);
  const hasFilters = Boolean(search || tipoFiltro || dateFrom || dateTo);
  const clearFilters = () => { setSearch(''); setTipoFiltro(''); setDateFrom(''); setDateTo(''); };

  const save = async () => {
    setSaving(true); setMessage(null);
    try {
      const created = await createMovimientoCaja(empresaId, { fecha, tipo, descripcion, monto: Number(monto), cuentaCajaId, cuentaContrapartidaId });
      setItems(current => [created, ...current]); setDescripcion(''); setMonto(''); setMessage(`Movimiento registrado y asiento ${created.asientoContableId} generado.`); setMessageKind('success');
    } catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo registrar el movimiento.'); setMessageKind('error'); } finally { setSaving(false); }
  };

  return <div className="page-stack financial-page cash-page">
    <div className="page-header transaction-page-header"><div><span className="eyebrow">Operaciones contables · Caja</span><h1>Caja</h1><p>Registra ingresos y egresos de efectivo con su efecto contable automático.</p></div><div className="transaction-context"><span>Período contable</span><strong>{ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'No seleccionado'}</strong><span className="transaction-company">{empresa?.razonSocial}</span></div></div>

    <section className="panel form-panel financial-form" id="nuevo-movimiento-caja"><div className="transaction-form-head"><div><span className="eyebrow">Registro contable</span><h2>Nuevo movimiento de caja</h2><p className="form-hint">Cada movimiento genera un asiento contable asociado.</p></div><span className={`movement-type-mark ${tipo === 'INGRESO' ? 'income' : 'expense'}`}><span aria-hidden="true">{tipo === 'INGRESO' ? '↑' : '↓'}</span>{tipo}</span></div>
      <div className="financial-section"><div className="transaction-section-title"><strong>Información del movimiento</strong><span>Fecha, naturaleza y concepto</span></div><div className="financial-grid financial-grid-movement"><label><span>Tipo de movimiento</span><select value={tipo} onChange={e => setTipo(e.target.value)}><option>INGRESO</option><option>EGRESO</option></select></label><label><span>Fecha</span><input type="date" value={fecha} onChange={e => setFecha(e.target.value)} /></label><label className="financial-field-wide"><span>Concepto</span><input value={descripcion} onChange={e => setDescripcion(e.target.value)} placeholder="Cobro a cliente / pago de servicio" /></label></div></div>
      <div className="financial-section"><div className="transaction-section-title"><strong>Origen y destino contable</strong><span>Cuentas relacionadas al movimiento</span></div><div className="financial-grid"><label><span>Cuenta de caja</span><select value={cuentaCajaId} onChange={e => setCuentaCajaId(Number(e.target.value))}>{cuentas.filter(c => c.codigo.startsWith('10')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label><label><span>Cuenta contrapartida</span><select value={cuentaContrapartidaId} onChange={e => setCuentaContrapartidaId(Number(e.target.value))}><option value={0}>Seleccione una cuenta</option>{cuentas.filter(c => !c.codigo.startsWith('10')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label></div></div>
      <div className="financial-section financial-section-last"><div className="transaction-section-title"><strong>Información financiera</strong><span>Importe aplicado al movimiento</span></div><div className="financial-amount-row"><label><span>Importe</span><input className="financial-amount-input" type="number" min="0.01" step="0.01" value={monto} onChange={e => setMonto(e.target.value)} placeholder="0.00" /></label><div className="financial-effect"><span>Efecto del registro</span><strong>{tipo === 'INGRESO' ? 'Ingreso de efectivo' : 'Egreso de efectivo'}</strong><small>La referencia del asiento se recibirá al registrar.</small></div></div></div>
      {message ? <div className={messageKind === 'success' ? 'success-box' : 'error-box'} role="status">{message}</div> : null}<div className="form-actions"><button className="primary-button" type="button" onClick={() => void save()} disabled={saving || loading}>{saving ? 'Guardando...' : 'Registrar movimiento'}</button></div>
    </section>

    <section className="panel table-panel financial-list-panel"><div className="transaction-list-head"><div><span className="eyebrow">Consulta histórica</span><h2>Movimientos de caja</h2><p className="form-hint">{hasFilters ? `${filteredItems.length} de ${items.length} movimientos visibles` : `${items.length} movimientos cargados`}</p></div><div className="financial-list-total"><span>Total visible · no oficial</span><div><strong className="income-text">Ingresos {money(totals.ingreso)}</strong><strong className="expense-text">Egresos {money(totals.egreso)}</strong><strong>Neto {money(totals.ingreso - totals.egreso)}</strong></div></div></div><div className="transaction-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Concepto, cuenta o ID" /></label><label><span>Tipo</span><select value={tipoFiltro} onChange={e => setTipoFiltro(e.target.value)}><option value="">Todos</option><option>INGRESO</option><option>EGRESO</option></select></label><label><span>Desde</span><input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} /></label><label><span>Hasta</span><input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} /></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>
      {loadError ? <div className="data-state data-state-error"><strong>No se pudo cargar caja</strong><span>{loadError}</span><button className="secondary-button" type="button" onClick={() => setReloadToken(value => value + 1)}>Reintentar</button></div> : loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando movimientos</strong><span>Consultando movimientos y cuentas contables.</span></div> : items.length === 0 ? <div className="data-state"><strong>Aún no hay movimientos de caja</strong><span>Los registros aparecerán aquí para consulta histórica.</span></div> : filteredItems.length === 0 ? <div className="data-state"><strong>No hay movimientos para estos filtros</strong><span>Prueba con otro concepto, tipo o período.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="financial-table"><thead><tr><th>Fecha</th><th>Tipo</th><th>Concepto</th><th>Cuenta / contrapartida</th><th className="numeric">Ingreso</th><th className="numeric">Egreso</th><th>Asiento</th></tr></thead><tbody>{filteredItems.map(item => <tr key={item.id}><td data-label="Fecha" className="date-cell">{dateLabel(item.fecha)}</td><td data-label="Tipo"><span className={`badge ${item.tipo === 'INGRESO' ? 'success' : 'muted'}`}><span aria-hidden="true">{item.tipo === 'INGRESO' ? '↑' : '↓'}</span> {item.tipo}</span></td><td data-label="Concepto"><strong>{item.descripcion}</strong><small>Movimiento #{item.id}</small></td><td data-label="Cuenta / contrapartida"><strong>{item.cuentaCaja}</strong><small>Contra: {item.cuentaContrapartida}</small></td><td data-label="Ingreso" className="numeric amount-cell income-text">{item.tipo === 'INGRESO' ? money(item.monto) : '—'}</td><td data-label="Egreso" className="numeric amount-cell expense-text">{item.tipo === 'EGRESO' ? money(item.monto) : '—'}</td><td data-label="Asiento"><span className={`badge ${item.asientoContableId ? 'success' : 'warning'}`}>{item.asientoContableId ? `#${item.asientoContableId}` : 'Sin referencia'}</span></td></tr>)}</tbody></table></div>}
    </section>
  </div>;
}

