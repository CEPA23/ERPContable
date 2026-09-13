import { Fragment, useEffect, useMemo, useState } from 'react';
import { getCuentas, type Cuenta } from '../api/contabilidad';
import { createCompra, getCompras, getProveedores, type Compra, type Proveedor } from '../api/compras';
import { getCatalogo, type Producto } from '../api/catalogo';
import { CorreccionesDocumentoButton } from '../components/CorreccionesDocumento';
import { estadosDocumento } from '../api/correcciones';
import { useAppSession } from '../context/AppSessionContext';

type Row = { cuentaContableId: number; productoServicioId: number; descripcion: string; cantidad: string; precioUnitario: string };
const today = new Date().toISOString().slice(0, 10);
const money = (value: number) => `S/ ${value.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
const dateLabel = (value: string) => new Date(value).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' });
const newRow = (): Row => ({ cuentaContableId: 0, productoServicioId: 0, descripcion: '', cantidad: '1', precioUnitario: '' });

export function ComprasPage() {
  const { empresa, ejercicio, periodo } = useAppSession();
  const empresaId = empresa?.id ?? 0;
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [productos, setProductos] = useState<Producto[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [compras, setCompras] = useState<Compra[]>([]);
  const [proveedorId, setProveedorId] = useState(0);
  const [fecha, setFecha] = useState(today);
  const [tipo, setTipo] = useState('FACTURA');
  const [serie, setSerie] = useState('F001');
  const [numero, setNumero] = useState('');
  const [formaPago, setFormaPago] = useState('CONTADO');
  const [cuentaPago, setCuentaPago] = useState(0);
  const [cuentaIgv, setCuentaIgv] = useState(0);
  const [igv, setIgv] = useState('');
  const [rows, setRows] = useState<Row[]>([newRow()]);
  const [message, setMessage] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState('');
  const [tipoFiltro, setTipoFiltro] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  const subtotal = useMemo(() => rows.reduce((sum, row) => sum + Number(row.cantidad || 0) * Number(row.precioUnitario || 0), 0), [rows]);
  const total = subtotal + Number(igv || 0);

  useEffect(() => {
    if (!empresaId) return;
    let active = true;
    setLoading(true); setLoadError(null);
    void Promise.all([getCuentas(empresaId), getProveedores(empresaId), getCompras(empresaId), getCatalogo(empresaId)]).then(([c, p, comprasData, catalogo]) => {
      if (!active) return;
      setCuentas(c); setProveedores(p); setCompras(comprasData); setProductos(catalogo.productos.filter(x => x.tipo === 'PRODUCTO' && x.activo));
      setCuentaPago(c.find(x => x.codigo.startsWith('10'))?.id ?? 0);
      setCuentaIgv(c.find(x => x.codigo.startsWith('40'))?.id ?? 0);
    }).catch(e => { if (active) setLoadError(e instanceof Error ? e.message : 'No se pudo cargar compras.'); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [empresaId, reloadToken]);

  const filteredCompras = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('es-PE');
    return compras.filter(compra => {
      const date = compra.fecha.slice(0, 10);
      const document = compra.comprobante.toLocaleLowerCase('es-PE');
      const matchesText = !normalized || document.includes(normalized) || compra.proveedor.toLocaleLowerCase('es-PE').includes(normalized) || String(compra.id).includes(normalized);
      const matchesType = !tipoFiltro || document.includes(tipoFiltro.toLocaleLowerCase('es-PE'));
      return matchesText && matchesType && (!dateFrom || date >= dateFrom) && (!dateTo || date <= dateTo);
    });
  }, [compras, dateFrom, dateTo, search, tipoFiltro]);

  const listTotals = useMemo(() => filteredCompras.reduce((sum, compra) => ({ subtotal: sum.subtotal + compra.subtotal - (compra.subtotalRevertido ?? 0), igv: sum.igv + compra.igv - (compra.igvRevertido ?? 0), total: sum.total + compra.total - (compra.subtotalRevertido ?? 0) - (compra.igvRevertido ?? 0) }), { subtotal: 0, igv: 0, total: 0 }), [filteredCompras]);
  const hasFilters = Boolean(search || tipoFiltro || dateFrom || dateTo);
  const clearFilters = () => { setSearch(''); setTipoFiltro(''); setDateFrom(''); setDateTo(''); };
  const update = (index: number, field: keyof Row, value: string | number) => setRows(items => items.map((row, i) => i === index ? { ...row, [field]: value } : row));

  const save = async () => {
    setMessage(null); setSaving(true);
    try {
      if (!proveedorId || !numero || !cuentaPago || !cuentaIgv || rows.some(x => !x.cuentaContableId || !x.descripcion || Number(x.cantidad) <= 0 || Number(x.precioUnitario) <= 0)) throw new Error('Completa proveedor, comprobante, cuentas y líneas válidas de la compra.');
      const created = await createCompra(empresaId, { fecha, proveedorId, tipoComprobante: tipo, serie, numero, formaPago, igv: Number(igv || 0), cuentaContrapartidaId: cuentaPago, cuentaIgvId: cuentaIgv, detalles: rows.map(x => ({ cuentaContableId: x.cuentaContableId, productoServicioId: x.productoServicioId || null, descripcion: x.descripcion, cantidad: Number(x.cantidad), precioUnitario: Number(x.precioUnitario) })) });
      setCompras(items => [created, ...items]); setNumero(''); setIgv(''); setRows([newRow()]); setMessage(`Compra registrada y asiento ${created.asientoContableId} generado.`);
    } catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo registrar la compra.'); } finally { setSaving(false); }
  };

  return <div className="page-stack transaction-page">
    <div className="page-header transaction-page-header"><div><span className="eyebrow">Operaciones contables · Compras</span><h1>Compras</h1><p>Registra comprobantes de proveedores y genera el asiento contable asociado.</p></div><div className="transaction-context"><span>Período contable</span><strong>{ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'No seleccionado'}</strong><span className="transaction-company">{empresa?.razonSocial}</span></div></div>

    <section className="panel form-panel transaction-form" id="nueva-compra">
      <div className="transaction-form-head"><div><span className="eyebrow">Registro documental</span><h2>Nueva compra</h2><p className="form-hint">La operación alimentará automáticamente el flujo contable.</p></div><span className="transaction-flow-badge">Compra → Asiento</span></div>
      <div className="transaction-section"><div className="transaction-section-title"><strong>Identificación del comprobante</strong><span>Documento y fecha de emisión</span></div><div className="transaction-grid transaction-grid-document"><label><span>Tipo de comprobante</span><select value={tipo} onChange={e => setTipo(e.target.value)}><option>FACTURA</option><option>BOLETA</option><option>RECIBO</option></select></label><label><span>Serie</span><input value={serie} onChange={e => setSerie(e.target.value)} /></label><label><span>Número</span><input value={numero} onChange={e => setNumero(e.target.value)} placeholder="000001" /></label><label><span>Fecha de emisión</span><input type="date" value={fecha} onChange={e => setFecha(e.target.value)} /></label></div></div>
      <div className="transaction-section"><div className="transaction-section-title"><strong>Tercero</strong><span>Proveedor relacionado con la operación</span></div><div className="transaction-grid"><label className="transaction-field-wide"><span>Proveedor</span><select value={proveedorId} onChange={e => setProveedorId(Number(e.target.value))}><option value={0}>Seleccione un proveedor</option>{proveedores.map(p => <option key={p.id} value={p.id}>{p.documento} · {p.razonSocial}</option>)}</select></label></div></div>
      <div className="transaction-section"><div className="transaction-section-title"><strong>Detalle contable e inventario</strong><span>Selecciona un producto para generar entrada de stock y contabilizarlo en la cuenta de inventario configurada.</span></div><div className="journal-lines transaction-lines"><div className="journal-line header purchase-line"><span>Producto (opcional)</span><span>Cuenta contable</span><span>Descripción</span><span>Cantidad</span><span>Precio unitario</span><span>Total</span></div>{rows.map((row, index) => <div className="journal-line purchase-line" key={index}><select value={row.productoServicioId} onChange={e => { const productId = Number(e.target.value); update(index, 'productoServicioId', productId); if (productId) { const inventoryAccount = cuentas.find(c => c.codigo === '20'); if (inventoryAccount) update(index, 'cuentaContableId', inventoryAccount.id); } }}><option value={0}>Sin inventario</option>{productos.map(p => <option key={p.id} value={p.id}>{p.codigo} - {p.nombre}</option>)}</select><select value={row.cuentaContableId} onChange={e => update(index, 'cuentaContableId', Number(e.target.value))}><option value={0}>Seleccione una cuenta</option>{cuentas.filter(c => c.tipo === 'GASTO' || c.tipo === 'ACTIVO').map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select><input value={row.descripcion} onChange={e => update(index, 'descripcion', e.target.value)} placeholder="Descripción" /><input type="number" min="0.01" step="0.0001" value={row.cantidad} onChange={e => update(index, 'cantidad', e.target.value)} aria-label={`Cantidad, línea ${index + 1}`} /><input type="number" min="0" step="0.0001" value={row.precioUnitario} onChange={e => update(index, 'precioUnitario', e.target.value)} aria-label={`Precio unitario, línea ${index + 1}`} /><output className="line-total">{money(Number(row.cantidad || 0) * Number(row.precioUnitario || 0))}</output></div>)}</div><button className="secondary-button add-line" type="button" onClick={() => setRows(items => [...items, newRow()])}>+ Agregar línea</button></div>
      <div className="transaction-section transaction-accounting-section"><div className="transaction-section-title"><strong>Importes e información contable</strong><span>Valores utilizados para generar el asiento</span></div><div className="transaction-summary-grid"><label><span>IGV</span><input type="number" min="0" step="0.01" value={igv} onChange={e => setIgv(e.target.value)} placeholder="0.00" /></label><label><span>Forma de pago</span><select value={formaPago} onChange={e => setFormaPago(e.target.value)}><option>CONTADO</option><option>CREDITO</option></select></label><label><span>Cuenta pago / crédito</span><select value={cuentaPago} onChange={e => setCuentaPago(Number(e.target.value))}>{cuentas.filter(c => c.codigo.startsWith('10') || c.codigo.startsWith('42')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label><label><span>Cuenta IGV</span><select value={cuentaIgv} onChange={e => setCuentaIgv(Number(e.target.value))}>{cuentas.filter(c => c.codigo.startsWith('40')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label><div className="transaction-totals"><span>Base imponible</span><strong>{money(subtotal)}</strong><span>IGV</span><strong>{money(Number(igv || 0))}</strong><span>Total documento</span><strong>{money(total)}</strong></div></div></div>
      {message ? <div className={message.includes('generado') ? 'success-box' : 'error-box'} role="status">{message}</div> : null}<div className="form-actions"><button className="primary-button" type="button" onClick={() => void save()} disabled={saving || loading}>{saving ? 'Guardando...' : 'Registrar compra'}</button></div>
    </section>

    <section className="panel table-panel transaction-list-panel"><div className="transaction-list-head"><div><span className="eyebrow">Consulta histórica</span><h2>Compras registradas</h2><p className="form-hint">{hasFilters ? `${filteredCompras.length} de ${compras.length} documentos visibles` : `${compras.length} documentos cargados`}</p></div><div className="transaction-list-total"><span>Total neto visible · no oficial</span><strong>{money(listTotals.total)}</strong><small>Base {money(listTotals.subtotal)} · IGV {money(listTotals.igv)}</small></div></div><div className="transaction-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Proveedor, comprobante o ID" /></label><label><span>Tipo</span><select value={tipoFiltro} onChange={e => setTipoFiltro(e.target.value)}><option value="">Todos</option><option>FACTURA</option><option>BOLETA</option><option>RECIBO</option></select></label><label><span>Desde</span><input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} /></label><label><span>Hasta</span><input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} /></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>
      {loadError ? <div className="data-state data-state-error"><strong>No se pudo cargar compras</strong><span>{loadError}</span><button className="secondary-button" type="button" onClick={() => setReloadToken(value => value + 1)}>Reintentar</button></div> : loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando compras</strong><span>Consultando documentos y referencias contables.</span></div> : compras.length === 0 ? <div className="data-state"><strong>Aún no hay compras registradas</strong><span>Los comprobantes registrados aparecerán aquí para consulta histórica.</span></div> : filteredCompras.length === 0 ? <div className="data-state"><strong>No hay compras para estos filtros</strong><span>Prueba con otro proveedor, comprobante o período.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="transaction-table"><thead><tr><th>Fecha</th><th>Comprobante</th><th>Proveedor</th><th className="numeric">Base</th><th className="numeric">IGV</th><th className="numeric">Total</th><th>Contabilización</th><th /></tr></thead><tbody>{filteredCompras.map(compra => { const expanded = expandedId === compra.id; return <Fragment key={compra.id}><tr className={expanded ? 'is-expanded' : ''}><td data-label="Fecha" className="date-cell">{dateLabel(compra.fecha)}</td><td data-label="Comprobante"><strong>{compra.comprobante}</strong><small>Compra #{compra.id}</small><span className="badge">{estadosDocumento[compra.estado] ?? 'Vigente'}</span></td><td data-label="Proveedor">{compra.proveedor}</td><td data-label="Base" className="numeric amount-cell">{money(compra.subtotal)}</td><td data-label="IGV" className="numeric amount-cell">{money(compra.igv)}</td><td data-label="Total" className="numeric amount-cell total-emphasis">{money(compra.total)}<small>{compra.formaPago}</small></td><td data-label="Contabilización"><span className={`badge ${compra.asientoContableId ? 'success' : 'warning'}`}>{compra.asientoContableId ? `Asiento #${compra.asientoContableId}` : 'Sin referencia'}</span></td><td data-label="Acciones" className="table-actions"><button className="text-button" type="button" onClick={() => setExpandedId(expanded ? null : compra.id)} aria-expanded={expanded}>{expanded ? 'Ocultar' : 'Ver detalle'}</button><CorreccionesDocumentoButton key={`${empresaId}-compra-${compra.id}`} empresaId={empresaId} tipo="compras" documentoId={compra.id} onSaved={doc => setCompras(items => items.map(item => item.id === doc.documentoId ? { ...item, estado: doc.estado, subtotalRevertido: doc.historial.reduce((sum, h) => sum + h.subtotal, 0), igvRevertido: doc.historial.reduce((sum, h) => sum + h.igv, 0) } : item))} /></td></tr>{expanded ? <tr className="transaction-detail-row"><td colSpan={8}><div className="transaction-detail"><strong>Detalle de compra #{compra.id}</strong>{compra.detalles.map((detalle, index) => <div className="transaction-detail-line" key={`${compra.id}-${index}`}><span>{detalle.cuenta} · {detalle.descripcion}</span><span>{detalle.cantidad} × {money(detalle.precioUnitario)}</span><strong>{money(detalle.total)}</strong></div>)}</div></td></tr> : null}</Fragment>; })}</tbody></table></div>}
    </section>
  </div>;
}

