import { Fragment, useEffect, useMemo, useState } from 'react';
import { createAsiento, getAsientos, getCuentas, type Asiento, type Cuenta } from '../api/contabilidad';
import { getEmpresas } from '../api/empresas';
import { useAppSession } from '../context/AppSessionContext';
import type { Empresa } from '../types/empresa';

type Row = { cuentaContableId: number; debe: string; haber: string };
const today = new Date().toISOString().slice(0, 10);
const meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

const money = (value: number) => `S/ ${value.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
const dateLabel = (value: string) => new Date(value).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' });

export function AsientosPage() {
  const { ejercicio, periodo } = useAppSession();
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [empresas, setEmpresas] = useState<Empresa[]>([]);
  const [empresaId, setEmpresaId] = useState(0);
  const [asientos, setAsientos] = useState<Asiento[]>([]);
  const [fecha, setFecha] = useState(today);
  const [glosa, setGlosa] = useState('');
  const [rows, setRows] = useState<Row[]>([{ cuentaContableId: 0, debe: '', haber: '' }, { cuentaContableId: 0, debe: '', haber: '' }]);
  const [message, setMessage] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loadingEmpresas, setLoadingEmpresas] = useState(true);
  const [loadingAsientos, setLoadingAsientos] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  const totals = useMemo(() => ({
    debe: rows.reduce((s, r) => s + Number(r.debe || 0), 0),
    haber: rows.reduce((s, r) => s + Number(r.haber || 0), 0),
  }), [rows]);

  useEffect(() => {
    let active = true;
    setLoadingEmpresas(true);
    setLoadError(null);
    void getEmpresas().then((items) => {
      if (!active) return;
      setEmpresas(items);
      if (items[0]) setEmpresaId(items[0].id);
    }).catch((e) => {
      if (active) setLoadError(e instanceof Error ? e.message : 'No se pudieron cargar las empresas.');
    }).finally(() => { if (active) setLoadingEmpresas(false); });
    return () => { active = false; };
  }, [reloadToken]);

  useEffect(() => {
    if (!empresaId) return;
    let active = true;
    setLoadingAsientos(true);
    setLoadError(null);
    setAsientos([]);
    void Promise.all([getCuentas(empresaId), getAsientos(empresaId)]).then(([c, a]) => {
      if (!active) return;
      setCuentas(c);
      setAsientos(a);
    }).catch((e) => {
      if (active) setLoadError(e instanceof Error ? e.message : 'No se pudo cargar contabilidad.');
    }).finally(() => { if (active) setLoadingAsientos(false); });
    return () => { active = false; };
  }, [empresaId, reloadToken]);

  const filteredAsientos = useMemo(() => {
    const normalizedSearch = search.trim().toLocaleLowerCase('es-PE');
    return asientos.filter((asiento) => {
      const date = asiento.fecha.slice(0, 10);
      const matchesText = !normalizedSearch || asiento.glosa.toLocaleLowerCase('es-PE').includes(normalizedSearch) || String(asiento.id).includes(normalizedSearch);
      const matchesFrom = !dateFrom || date >= dateFrom;
      const matchesTo = !dateTo || date <= dateTo;
      return matchesText && matchesFrom && matchesTo;
    });
  }, [asientos, dateFrom, dateTo, search]);

  const listTotals = useMemo(() => filteredAsientos.reduce((total, asiento) => ({
    debe: total.debe + asiento.totalDebe,
    haber: total.haber + asiento.totalHaber,
  }), { debe: 0, haber: 0 }), [filteredAsientos]);

  const updateRow = (index: number, field: keyof Row, value: string | number) => setRows((current) => current.map((row, i) => i === index ? { ...row, [field]: value, ...(field === 'debe' && value ? { haber: '' } : {}), ...(field === 'haber' && value ? { debe: '' } : {}) } : row));

  const save = async () => {
    setMessage(null);
    if (!glosa.trim() || Math.abs(totals.debe - totals.haber) > 0.005) { setMessage('Completa la glosa y verifica que el debe sea igual al haber.'); return; }
    setSaving(true);
    try {
      if (!empresaId) { setMessage('Seleccione una empresa antes de registrar el asiento.'); return; }
      const created = await createAsiento(empresaId, { fecha, glosa, detalles: rows.map((r) => ({ cuentaContableId: r.cuentaContableId, debe: Number(r.debe || 0), haber: Number(r.haber || 0) })) });
      setAsientos((current) => [created, ...current]);
      setGlosa('');
      setRows([{ cuentaContableId: 0, debe: '', haber: '' }, { cuentaContableId: 0, debe: '', haber: '' }]);
      setMessage('Asiento registrado correctamente.');
    } catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo guardar el asiento.'); } finally { setSaving(false); }
  };

  const clearFilters = () => { setSearch(''); setDateFrom(''); setDateTo(''); };
  const hasFilters = Boolean(search || dateFrom || dateTo);
  const isLoading = loadingEmpresas || loadingAsientos;

  return <div className="page-stack journal-page">
    <div className="page-header journal-page-header">
      <div>
        <span className="eyebrow">Contabilidad · Registro diario</span>
        <h1>Asientos contables</h1>
        <p>Registra y consulta operaciones aplicando la partida doble.</p>
      </div>
      <div className="journal-page-actions">
        <div className="journal-context"><span>Contexto activo</span><strong>{ejercicio ?? '—'} · {periodo ? meses[periodo - 1] : 'Período no seleccionado'}</strong></div>
        <select className="company-selector" value={empresaId} onChange={(e) => setEmpresaId(Number(e.target.value))} aria-label="Empresa del asiento"><option value={0}>Seleccione una empresa</option>{empresas.map((empresa) => <option key={empresa.id} value={empresa.id}>{empresa.razonSocial}</option>)}</select>
        <a className="primary-button" href="#nuevo-asiento">Nuevo asiento</a>
      </div>
    </div>

    <section className="panel form-panel journal-form" id="nuevo-asiento">
      <div className="journal-head"><div><h2>Nuevo asiento</h2><p className="form-hint">Registra una operación manual en la empresa seleccionada.</p></div><span className={`balance-pill ${Math.abs(totals.debe - totals.haber) < .005 ? 'balanced' : ''}`}>Debe: {money(totals.debe)} <span aria-hidden="true">·</span> Haber: {money(totals.haber)}</span></div>
      <div className="journal-meta"><label><span>Fecha</span><input type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} /></label><label><span>Glosa / descripción</span><input value={glosa} onChange={(e) => setGlosa(e.target.value)} placeholder="Registro de operación" /></label></div>
      <div className="journal-lines"><div className="journal-line header"><span>Cuenta contable</span><span>Debe</span><span>Haber</span><span /></div>{rows.map((row, index) => <div className="journal-line" key={index}><select value={row.cuentaContableId} onChange={(e) => updateRow(index, 'cuentaContableId', Number(e.target.value))}><option value={0}>Seleccione una cuenta</option>{cuentas.map((c) => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select><input type="number" min="0" step="0.01" value={row.debe} onChange={(e) => updateRow(index, 'debe', e.target.value)} placeholder="0.00" aria-label={`Debe, línea ${index + 1}`} /><input type="number" min="0" step="0.01" value={row.haber} onChange={(e) => updateRow(index, 'haber', e.target.value)} placeholder="0.00" aria-label={`Haber, línea ${index + 1}`} /><button className="remove-line" type="button" onClick={() => setRows((current) => current.filter((_, i) => i !== index))} aria-label={`Eliminar línea ${index + 1}`}>×</button></div>)}</div>
      <button className="secondary-button add-line" type="button" onClick={() => setRows((current) => [...current, { cuentaContableId: 0, debe: '', haber: '' }])}>+ Agregar línea</button>
      {message ? <div className={message.includes('correctamente') ? 'success-box' : 'error-box'} role="status">{message}</div> : null}<div className="form-actions"><button className="primary-button" type="button" onClick={() => void save()} disabled={saving || loadingAsientos}>{saving ? 'Guardando...' : 'Registrar asiento'}</button></div>
    </section>

      <section className="panel table-panel journal-list-panel">
      <div className="journal-list-head journal-list-toolbar"><div><span className="eyebrow">Consulta de movimientos</span><h2>Asientos registrados</h2><p className="form-hint">{hasFilters ? `${filteredAsientos.length} de ${asientos.length} resultados` : `${asientos.length} asientos cargados`}</p></div><div className="journal-list-total"><span>Total mostrado</span><strong>{money(listTotals.debe)}</strong><strong>{money(listTotals.haber)}</strong></div></div>
      <div className="journal-filters" role="search"><label className="journal-search"><span>Buscar asiento</span><input type="search" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Glosa o número de asiento" /></label><label><span>Desde</span><input type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} /></label><label><span>Hasta</span><input type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} /></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>

      {loadError ? <div className="data-state data-state-error"><strong>No se pudo cargar la información</strong><span>{loadError}</span><button className="secondary-button" type="button" onClick={() => setReloadToken((current) => current + 1)}>Reintentar</button></div> : isLoading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando asientos</strong><span>Consultando la información contable de la empresa.</span></div> : asientos.length === 0 ? <div className="data-state"><strong>Aún no hay asientos registrados</strong><span>Registra el primer asiento para comenzar a consultar movimientos.</span></div> : filteredAsientos.length === 0 ? <div className="data-state"><strong>No hay resultados para estos filtros</strong><span>Prueba con otra glosa, número o rango de fechas.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="journal-table"><thead><tr><th scope="col">Fecha</th><th scope="col">Asiento / glosa</th><th scope="col" className="numeric">Debe</th><th scope="col" className="numeric">Haber</th><th scope="col">Estado</th><th scope="col" className="table-action-heading"><span className="sr-only">Acciones</span></th></tr></thead><tbody>{filteredAsientos.map((asiento) => { const balanced = Math.abs(asiento.totalDebe - asiento.totalHaber) < .005; const expanded = expandedId === asiento.id; return <Fragment key={asiento.id}>
        <tr className={expanded ? 'is-expanded' : ''}>
          <td data-label="Fecha" className="date-cell">{dateLabel(asiento.fecha)}</td>
          <td data-label="Asiento / glosa"><strong>#{asiento.id}</strong><span className="journal-glosa">{asiento.glosa}</span><small>{asiento.detalles.length} {asiento.detalles.length === 1 ? 'línea' : 'líneas'}</small></td>
          <td data-label="Debe" className="numeric amount-cell">{money(asiento.totalDebe)}</td>
          <td data-label="Haber" className="numeric amount-cell">{money(asiento.totalHaber)}</td>
          <td data-label="Estado"><span className={`badge ${balanced ? 'success' : 'warning'}`}>{balanced ? 'Balanceado' : 'Revisar'}</span></td>
          <td data-label="Acciones" className="table-actions"><button className="text-button" type="button" onClick={() => setExpandedId(expanded ? null : asiento.id)} aria-expanded={expanded}>{expanded ? 'Ocultar' : 'Ver detalle'}</button></td>
        </tr>
        {expanded ? <tr className="journal-detail-row"><td colSpan={6}><div className="journal-detail"><strong>Detalle del asiento #{asiento.id}</strong><div className="journal-detail-grid">{asiento.detalles.map((detalle, index) => <div className="journal-detail-line" key={`${asiento.id}-${index}`}><span>{detalle.cuenta}</span><span className="numeric">{money(detalle.debe)}</span><span className="numeric">{money(detalle.haber)}</span></div>)}</div></div></td></tr> : null}
      </Fragment>; })}</tbody></table></div>}
    </section>
  </div>;
}

