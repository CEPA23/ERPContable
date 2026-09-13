import { useEffect, useMemo, useState } from 'react';
import { getCuentas, type Cuenta } from '../api/contabilidad';
import { createCuentaBancaria, createMovimientoBanco, getCuentasBancarias, getMovimientosBanco, type CuentaBancaria, type MovimientoBanco } from '../api/bancos';
import { useAppSession } from '../context/AppSessionContext';

const today = new Date().toISOString().slice(0, 10);
const money = (value: number) => `S/ ${value.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
const dateLabel = (value: string) => new Date(value).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' });

export function BancosPage() {
  const { empresa, ejercicio, periodo } = useAppSession();
  const empresaId = empresa?.id ?? 0;
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [bancos, setBancos] = useState<CuentaBancaria[]>([]);
  const [movimientos, setMovimientos] = useState<MovimientoBanco[]>([]);
  const [banco, setBanco] = useState('');
  const [numeroCuenta, setNumeroCuenta] = useState('');
  const [cuentaContableId, setCuentaContableId] = useState(0);
  const [cuentaBancariaId, setCuentaBancariaId] = useState(0);
  const [tipo, setTipo] = useState('INGRESO');
  const [fecha] = useState(today);
  const [descripcion, setDescripcion] = useState('');
  const [monto, setMonto] = useState('');
  const [contrapartida, setContrapartida] = useState(0);
  const [message, setMessage] = useState<string | null>(null);
  const [messageKind, setMessageKind] = useState<'success' | 'error'>('success');
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [savingAccount, setSavingAccount] = useState(false);
  const [savingMovement, setSavingMovement] = useState(false);
  const [search, setSearch] = useState('');
  const [tipoFiltro, setTipoFiltro] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    if (!empresaId) return;
    let active = true;
    setLoading(true); setLoadError(null);
    void Promise.all([getCuentas(empresaId), getCuentasBancarias(empresaId), getMovimientosBanco(empresaId)]).then(([c, b, m]) => {
      if (!active) return;
      setCuentas(c); setBancos(b); setMovimientos(m); setCuentaContableId(c.find(x => x.codigo.startsWith('10'))?.id ?? 0); if (b[0]) setCuentaBancariaId(b[0].id);
    }).catch(e => { if (active) setLoadError(e instanceof Error ? e.message : 'No se pudo cargar bancos.'); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [empresaId, reloadToken]);

  const filteredMovimientos = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('es-PE');
    return movimientos.filter(item => {
      const searchable = `${item.descripcion} ${item.banco} ${item.numeroCuenta} ${item.cuentaContrapartida} ${item.id}`.toLocaleLowerCase('es-PE');
      const date = item.fecha.slice(0, 10);
      return (!normalized || searchable.includes(normalized)) && (!tipoFiltro || item.tipo === tipoFiltro) && (!dateFrom || date >= dateFrom) && (!dateTo || date <= dateTo);
    });
  }, [dateFrom, dateTo, movimientos, search, tipoFiltro]);

  const totals = useMemo(() => filteredMovimientos.reduce((sum, item) => item.tipo === 'INGRESO' ? { ingreso: sum.ingreso + item.monto, egreso: sum.egreso } : { ingreso: sum.ingreso, egreso: sum.egreso + item.monto }, { ingreso: 0, egreso: 0 }), [filteredMovimientos]);
  const hasFilters = Boolean(search || tipoFiltro || dateFrom || dateTo);
  const clearFilters = () => { setSearch(''); setTipoFiltro(''); setDateFrom(''); setDateTo(''); };

  const registrarCuenta = async () => {
    setSavingAccount(true); setMessage(null);
    try { const item = await createCuentaBancaria(empresaId, { banco, numeroCuenta, moneda: 'PEN', cuentaContableId }); setBancos(current => [...current, item]); setCuentaBancariaId(item.id); setBanco(''); setNumeroCuenta(''); setMessage('Cuenta bancaria registrada.'); setMessageKind('success'); }
    catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo registrar la cuenta bancaria.'); setMessageKind('error'); } finally { setSavingAccount(false); }
  };

  const registrarMovimiento = async () => {
    setSavingMovement(true); setMessage(null);
    try { const item = await createMovimientoBanco(empresaId, { cuentaBancariaId, fecha, tipo, descripcion, monto: Number(monto), cuentaContrapartidaId: contrapartida }); setMovimientos(current => [item, ...current]); setDescripcion(''); setMonto(''); setMessage(`Movimiento registrado y asiento ${item.asientoContableId} generado.`); setMessageKind('success'); }
    catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo registrar el movimiento.'); setMessageKind('error'); } finally { setSavingMovement(false); }
  };

  return <div className="page-stack financial-page bank-page">
    <div className="page-header transaction-page-header"><div><span className="eyebrow">Operaciones contables · Bancos</span><h1>Bancos</h1><p>Administra cuentas bancarias y registra sus movimientos con referencia contable.</p></div><div className="transaction-context"><span>Período contable</span><strong>{ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'No seleccionado'}</strong><span className="transaction-company">{empresa?.razonSocial} · {bancos.length} cuenta{bancos.length === 1 ? '' : 's'} bancaria{bancos.length === 1 ? '' : 's'}</span></div></div>

    <section className="panel form-panel financial-form bank-account-form"><div className="transaction-form-head"><div><span className="eyebrow">Configuración contable</span><h2>Nueva cuenta bancaria</h2><p className="form-hint">Vincula la cuenta bancaria con una cuenta contable existente.</p></div><span className="transaction-flow-badge">Banco → Cuenta contable</span></div><div className="financial-grid bank-account-grid"><label><span>Banco</span><input value={banco} onChange={e => setBanco(e.target.value)} placeholder="BCP" /></label><label><span>Número de cuenta</span><input value={numeroCuenta} onChange={e => setNumeroCuenta(e.target.value)} /></label><label><span>Cuenta contable</span><select value={cuentaContableId} onChange={e => setCuentaContableId(Number(e.target.value))}>{cuentas.filter(c => c.codigo.startsWith('10')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label><div className="form-actions bank-account-action"><button className="secondary-button" type="button" onClick={() => void registrarCuenta()} disabled={savingAccount || loading}>{savingAccount ? 'Guardando...' : 'Registrar cuenta bancaria'}</button></div></div>{bancos.length > 0 ? <div className="bank-account-list"><span>Cuentas registradas</span>{bancos.map(item => <span className="bank-account-chip" key={item.id}><strong>{item.banco}</strong> · {item.numeroCuenta} · {item.moneda}</span>)}</div> : null}</section>

    <section className="panel form-panel financial-form" id="nuevo-movimiento-banco"><div className="transaction-form-head"><div><span className="eyebrow">Registro contable</span><h2>Nuevo movimiento bancario</h2><p className="form-hint">Registra el movimiento y genera el asiento asociado.</p></div><span className={`movement-type-mark ${tipo === 'INGRESO' ? 'income' : 'expense'}`}><span aria-hidden="true">{tipo === 'INGRESO' ? '↑' : '↓'}</span>{tipo}</span></div><div className="financial-section"><div className="transaction-section-title"><strong>Información del movimiento</strong><span>Cuenta, fecha, naturaleza y concepto</span></div><div className="financial-grid financial-grid-movement"><label><span>Cuenta bancaria</span><select value={cuentaBancariaId} onChange={e => setCuentaBancariaId(Number(e.target.value))}><option value={0}>Seleccione una cuenta</option>{bancos.map(b => <option key={b.id} value={b.id}>{b.banco} · {b.numeroCuenta}</option>)}</select></label><label><span>Tipo de movimiento</span><select value={tipo} onChange={e => setTipo(e.target.value)}><option>INGRESO</option><option>EGRESO</option></select></label><label><span>Fecha</span><input type="date" value={fecha} readOnly /></label><label className="financial-field-wide"><span>Concepto</span><input value={descripcion} onChange={e => setDescripcion(e.target.value)} placeholder="Depósito / pago / transferencia registrada" /></label></div></div><div className="financial-section"><div className="transaction-section-title"><strong>Origen y destino contable</strong><span>Cuenta contrapartida del movimiento</span></div><div className="financial-grid"><label className="financial-field-wide"><span>Cuenta contrapartida</span><select value={contrapartida} onChange={e => setContrapartida(Number(e.target.value))}><option value={0}>Seleccione una cuenta</option>{cuentas.filter(c => !c.codigo.startsWith('10')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label></div></div><div className="financial-section financial-section-last"><div className="transaction-section-title"><strong>Información financiera</strong><span>Importe aplicado al movimiento</span></div><div className="financial-amount-row"><label><span>Importe</span><input className="financial-amount-input" type="number" min="0.01" step="0.01" value={monto} onChange={e => setMonto(e.target.value)} placeholder="0.00" /></label><div className="financial-effect"><span>Efecto del registro</span><strong>{tipo === 'INGRESO' ? 'Ingreso bancario' : 'Egreso bancario'}</strong><small>La referencia del asiento se recibirá al registrar.</small></div></div></div>{message ? <div className={messageKind === 'success' ? 'success-box' : 'error-box'} role="status">{message}</div> : null}<div className="form-actions"><button className="primary-button" type="button" onClick={() => void registrarMovimiento()} disabled={savingMovement || loading}>{savingMovement ? 'Guardando...' : 'Registrar movimiento'}</button></div></section>

    <section className="panel table-panel financial-list-panel"><div className="transaction-list-head"><div><span className="eyebrow">Consulta histórica</span><h2>Movimientos bancarios</h2><p className="form-hint">{hasFilters ? `${filteredMovimientos.length} de ${movimientos.length} movimientos visibles` : `${movimientos.length} movimientos cargados`}</p></div><div className="financial-list-total"><span>Total visible · no oficial</span><div><strong className="income-text">Ingresos {money(totals.ingreso)}</strong><strong className="expense-text">Egresos {money(totals.egreso)}</strong><strong>Neto {money(totals.ingreso - totals.egreso)}</strong></div></div></div><div className="transaction-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Banco, cuenta, concepto o ID" /></label><label><span>Tipo</span><select value={tipoFiltro} onChange={e => setTipoFiltro(e.target.value)}><option value="">Todos</option><option>INGRESO</option><option>EGRESO</option></select></label><label><span>Desde</span><input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} /></label><label><span>Hasta</span><input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} /></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>{loadError ? <div className="data-state data-state-error"><strong>No se pudo cargar bancos</strong><span>{loadError}</span><button className="secondary-button" type="button" onClick={() => setReloadToken(value => value + 1)}>Reintentar</button></div> : loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando movimientos</strong><span>Consultando cuentas, bancos y referencias contables.</span></div> : movimientos.length === 0 ? <div className="data-state"><strong>Aún no hay movimientos bancarios</strong><span>Los registros aparecerán aquí para consulta histórica.</span></div> : filteredMovimientos.length === 0 ? <div className="data-state"><strong>No hay movimientos para estos filtros</strong><span>Prueba con otro banco, tipo o período.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="financial-table"><thead><tr><th>Fecha</th><th>Banco / cuenta</th><th>Tipo</th><th>Concepto</th><th>Contrapartida</th><th className="numeric">Ingreso</th><th className="numeric">Egreso</th><th>Asiento</th></tr></thead><tbody>{filteredMovimientos.map(item => <tr key={item.id}><td data-label="Fecha" className="date-cell">{dateLabel(item.fecha)}</td><td data-label="Banco / cuenta"><strong>{item.banco}</strong><small>{item.numeroCuenta}</small></td><td data-label="Tipo"><span className={`badge ${item.tipo === 'INGRESO' ? 'success' : 'muted'}`}><span aria-hidden="true">{item.tipo === 'INGRESO' ? '↑' : '↓'}</span> {item.tipo}</span></td><td data-label="Concepto"><strong>{item.descripcion}</strong><small>Movimiento #{item.id}</small></td><td data-label="Contrapartida">{item.cuentaContrapartida}</td><td data-label="Ingreso" className="numeric amount-cell income-text">{item.tipo === 'INGRESO' ? money(item.monto) : '—'}</td><td data-label="Egreso" className="numeric amount-cell expense-text">{item.tipo === 'EGRESO' ? money(item.monto) : '—'}</td><td data-label="Asiento"><span className={`badge ${item.asientoContableId ? 'success' : 'warning'}`}>{item.asientoContableId ? `#${item.asientoContableId}` : 'Sin referencia'}</span></td></tr>)}</tbody></table></div>}
    </section>
  </div>;
}

