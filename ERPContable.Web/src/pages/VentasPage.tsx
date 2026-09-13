import { useEffect, useMemo, useState } from 'react';
import { getCuentas, type Cuenta } from '../api/contabilidad';
import { createVenta, getClientes, getVentas, type Cliente, type Venta } from '../api/ventas';
import { getCatalogo, type Producto } from '../api/catalogo';
import { CorreccionesDocumentoButton } from '../components/CorreccionesDocumento';
import { estadosDocumento } from '../api/correcciones';
import { useAppSession } from '../context/AppSessionContext';

type Row = { cuentaContableId: number; productoServicioId: number; descripcion: string; cantidad: string; precioUnitario: string };
const today = new Date().toISOString().slice(0, 10);
const money = (value: number) => `S/ ${value.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
const dateLabel = (value: string) => new Date(value).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' });
const newRow = (): Row => ({ cuentaContableId: 0, productoServicioId: 0, descripcion: '', cantidad: '1', precioUnitario: '' });

export function VentasPage() {
  const { empresa, ejercicio, periodo } = useAppSession();
  const empresaId = empresa?.id ?? 0;
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [productos, setProductos] = useState<Producto[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [ventas, setVentas] = useState<Venta[]>([]);
  const [clienteId, setClienteId] = useState(0);
  const [fecha, setFecha] = useState(today);
  const [tipo, setTipo] = useState('FACTURA');
  const [serie, setSerie] = useState('F001');
  const [numero, setNumero] = useState('');
  const [formaPago, setFormaPago] = useState('CONTADO');
  const [cuentaCobro, setCuentaCobro] = useState(0);
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
  const [reloadToken, setReloadToken] = useState(0);

  const subtotal = useMemo(() => rows.reduce((sum, row) => sum + Number(row.cantidad || 0) * Number(row.precioUnitario || 0), 0), [rows]);
  const total = subtotal + Number(igv || 0);

  useEffect(() => {
    if (!empresaId) return;
    let active = true;
    setLoading(true); setLoadError(null);
    void Promise.all([getCuentas(empresaId), getClientes(empresaId), getVentas(empresaId), getCatalogo(empresaId)]).then(([c, clients, sales, catalogo]) => {
      if (!active) return;
      setCuentas(c); setClientes(clients); setVentas(sales); setProductos(catalogo.productos.filter(x => x.tipo === 'PRODUCTO' && x.activo));
      setCuentaCobro(c.find(x => x.codigo.startsWith('10'))?.id ?? 0);
      setCuentaIgv(c.find(x => x.codigo.startsWith('40'))?.id ?? 0);
    }).catch(e => { if (active) setLoadError(e instanceof Error ? e.message : 'No se pudo cargar ventas.'); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [empresaId, reloadToken]);

  const filteredVentas = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('es-PE');
    return ventas.filter(venta => {
      const date = venta.fecha.slice(0, 10);
      const document = venta.comprobante.toLocaleLowerCase('es-PE');
      const matchesText = !normalized || document.includes(normalized) || venta.cliente.toLocaleLowerCase('es-PE').includes(normalized) || String(venta.id).includes(normalized);
      const matchesType = !tipoFiltro || document.includes(tipoFiltro.toLocaleLowerCase('es-PE'));
      return matchesText && matchesType && (!dateFrom || date >= dateFrom) && (!dateTo || date <= dateTo);
    });
  }, [dateFrom, dateTo, search, tipoFiltro, ventas]);

  const listTotals = useMemo(() => filteredVentas.reduce((sum, venta) => ({ subtotal: sum.subtotal + venta.subtotal - (venta.subtotalRevertido ?? 0), igv: sum.igv + venta.igv - (venta.igvRevertido ?? 0), total: sum.total + venta.total - (venta.subtotalRevertido ?? 0) - (venta.igvRevertido ?? 0) }), { subtotal: 0, igv: 0, total: 0 }), [filteredVentas]);
  const hasFilters = Boolean(search || tipoFiltro || dateFrom || dateTo);
  const clearFilters = () => { setSearch(''); setTipoFiltro(''); setDateFrom(''); setDateTo(''); };
  const update = (index: number, field: keyof Row, value: string | number) => setRows(items => items.map((row, i) => i === index ? { ...row, [field]: value } : row));

  const save = async () => {
    setMessage(null); setSaving(true);
    try {
      if (!clienteId || !numero || !cuentaCobro || !cuentaIgv || rows.some(x => !x.cuentaContableId || !x.descripcion || Number(x.cantidad) <= 0 || Number(x.precioUnitario) <= 0)) throw new Error('Completa cliente, comprobante, cuentas y líneas válidas de la venta.');
      const created = await createVenta(empresaId, { fecha, clienteId, tipoComprobante: tipo, serie, numero, formaPago, igv: Number(igv || 0), cuentaContrapartidaId: cuentaCobro, cuentaIgvId: cuentaIgv, detalles: rows.map(x => ({ cuentaContableId: x.cuentaContableId, productoServicioId: x.productoServicioId || null, descripcion: x.descripcion, cantidad: Number(x.cantidad), precioUnitario: Number(x.precioUnitario) })) });
      setVentas(items => [created, ...items]); setNumero(''); setIgv(''); setRows([newRow()]); setMessage(created.asientoCostoVentaId ? `Venta registrada. Asientos ${created.asientoContableId} y costo de ventas ${created.asientoCostoVentaId} generados.` : `Venta registrada y asiento ${created.asientoContableId} generado.`);
    } catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo registrar la venta.'); } finally { setSaving(false); }
  };

  return <div className="page-stack transaction-page">
    <div className="page-header transaction-page-header"><div><span className="eyebrow">Operaciones contables · Ventas</span><h1>Ventas</h1><p>Registra comprobantes de clientes y genera el asiento contable asociado.</p></div><div className="transaction-context"><span>Período contable</span><strong>{ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'No seleccionado'}</strong><span className="transaction-company">{empresa?.razonSocial}</span></div></div>

    <section className="panel form-panel transaction-form" id="nueva-venta">
      <div className="transaction-form-head"><div><span className="eyebrow">Registro documental</span><h2>Nueva venta</h2><p className="form-hint">La operación alimentará automáticamente el flujo contable.</p></div><span className="transaction-flow-badge">Venta → Asiento</span></div>
      <div className="transaction-section"><div className="transaction-section-title"><strong>Identificación del comprobante</strong><span>Documento y fecha de emisión</span></div><div className="transaction-grid transaction-grid-document"><label><span>Tipo de comprobante</span><select value={tipo} onChange={e => setTipo(e.target.value)}><option>FACTURA</option><option>BOLETA</option><option>RECIBO</option></select></label><label><span>Serie</span><input value={serie} onChange={e => setSerie(e.target.value)} /></label><label><span>Número</span><input value={numero} onChange={e => setNumero(e.target.value)} placeholder="000001" /></label><label><span>Fecha de emisión</span><input type="date" value={fecha} onChange={e => setFecha(e.target.value)} /></label></div></div>
      <div className="transaction-section"><div className="transaction-section-title"><strong>Tercero</strong><span>Cliente relacionado con la operación</span></div><div className="transaction-grid"><label className="transaction-field-wide"><span>Cliente</span><select value={clienteId} onChange={e => setClienteId(Number(e.target.value))}><option value={0}>Seleccione un cliente</option>{clientes.map(c => <option key={c.id} value={c.id}>{c.documento} · {c.razonSocial}</option>)}</select></label></div></div>
      <div className="transaction-section"><div className="transaction-section-title"><strong>Detalle contable e inventario</strong><span>Selecciona un producto para que la venta genere una salida automática de stock.</span></div><div className="journal-lines transaction-lines"><div className="journal-line header purchase-line"><span>Producto (opcional)</span><span>Cuenta contable</span><span>Descripción</span><span>Cantidad</span><span>Precio unitario</span><span>Total</span></div>{rows.map((row, index) => <div className="journal-line purchase-line" key={index}><select value={row.productoServicioId} onChange={e => update(index, 'productoServicioId', Number(e.target.value))}><option value={0}>Sin inventario</option>{productos.map(p => <option key={p.id} value={p.id}>{p.codigo} - {p.nombre}</option>)}</select><select value={row.cuentaContableId} onChange={e => update(index, 'cuentaContableId', Number(e.target.value))}><option value={0}>Seleccione una cuenta</option>{cuentas.filter(c => c.tipo === 'INGRESO').map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select><input value={row.descripcion} onChange={e => update(index, 'descripcion', e.target.value)} placeholder="Descripción" /><input type="number" min="0.01" step="0.0001" value={row.cantidad} onChange={e => update(index, 'cantidad', e.target.value)} aria-label={`Cantidad, línea ${index + 1}`} /><input type="number" min="0" step="0.0001" value={row.precioUnitario} onChange={e => update(index, 'precioUnitario', e.target.value)} aria-label={`Precio unitario, línea ${index + 1}`} /><output className="line-total">{money(Number(row.cantidad || 0) * Number(row.precioUnitario || 0))}</output></div>)}</div><button className="secondary-button add-line" type="button" onClick={() => setRows(items => [...items, newRow()])}>+ Agregar línea</button></div>
      <div className="transaction-section transaction-accounting-section"><div className="transaction-section-title"><strong>Importes e información contable</strong><span>Valores utilizados para generar el asiento</span></div><div className="transaction-summary-grid"><label><span>IGV</span><input type="number" min="0" step="0.01" value={igv} onChange={e => setIgv(e.target.value)} placeholder="0.00" /></label><label><span>Forma de pago</span><select value={formaPago} onChange={e => setFormaPago(e.target.value)}><option>CONTADO</option><option>CREDITO</option></select></label><label><span>Cuenta cobro / crédito</span><select value={cuentaCobro} onChange={e => setCuentaCobro(Number(e.target.value))}>{cuentas.filter(c => c.codigo.startsWith('10') || c.codigo.startsWith('12')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label><label><span>Cuenta IGV</span><select value={cuentaIgv} onChange={e => setCuentaIgv(Number(e.target.value))}>{cuentas.filter(c => c.codigo.startsWith('40')).map(c => <option key={c.id} value={c.id}>{c.codigo} - {c.nombre}</option>)}</select></label><div className="transaction-totals"><span>Base imponible</span><strong>{money(subtotal)}</strong><span>IGV</span><strong>{money(Number(igv || 0))}</strong><span>Total documento</span><strong>{money(total)}</strong></div></div></div>
      {message ? <div className={message.includes('generado') ? 'success-box' : 'error-box'} role="status">{message}</div> : null}<div className="form-actions"><button className="primary-button" type="button" onClick={() => void save()} disabled={saving || loading}>{saving ? 'Guardando...' : 'Registrar venta'}</button></div>
    </section>

    <section className="panel table-panel transaction-list-panel"><div className="transaction-list-head"><div><span className="eyebrow">Consulta histórica</span><h2>Ventas registradas</h2><p className="form-hint">{hasFilters ? `${filteredVentas.length} de ${ventas.length} documentos visibles` : `${ventas.length} documentos cargados`}</p></div><div className="transaction-list-total"><span>Total neto visible · no oficial</span><strong>{money(listTotals.total)}</strong><small>Base {money(listTotals.subtotal)} · IGV {money(listTotals.igv)}</small></div></div><div className="transaction-filters"><label className="transaction-search"><span>Buscar</span><input type="search" value={search} onChange={e => setSearch(e.target.value)} placeholder="Cliente, comprobante o ID" /></label><label><span>Tipo</span><select value={tipoFiltro} onChange={e => setTipoFiltro(e.target.value)}><option value="">Todos</option><option>FACTURA</option><option>BOLETA</option><option>RECIBO</option></select></label><label><span>Desde</span><input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} /></label><label><span>Hasta</span><input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} /></label>{hasFilters ? <button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button> : null}</div>
      {loadError ? <div className="data-state data-state-error"><strong>No se pudo cargar ventas</strong><span>{loadError}</span><button className="secondary-button" type="button" onClick={() => setReloadToken(value => value + 1)}>Reintentar</button></div> : loading ? <div className="data-state"><span className="loading-indicator" aria-hidden="true" /><strong>Cargando ventas</strong><span>Consultando documentos y referencias contables.</span></div> : ventas.length === 0 ? <div className="data-state"><strong>Aún no hay ventas registradas</strong><span>Los comprobantes registrados aparecerán aquí para consulta histórica.</span></div> : filteredVentas.length === 0 ? <div className="data-state"><strong>No hay ventas para estos filtros</strong><span>Prueba con otro cliente, comprobante o período.</span><button className="text-button" type="button" onClick={clearFilters}>Limpiar filtros</button></div> : <div className="table-wrap"><table className="transaction-table"><thead><tr><th>Fecha</th><th>Comprobante</th><th>Cliente</th><th className="numeric">Base</th><th className="numeric">IGV</th><th className="numeric">Total</th><th>Contabilización</th><th>Acciones</th></tr></thead><tbody>{filteredVentas.map(venta => <tr key={venta.id}><td data-label="Fecha" className="date-cell">{dateLabel(venta.fecha)}</td><td data-label="Comprobante"><strong>{venta.comprobante}</strong><small>Venta #{venta.id}</small><span className="badge">{estadosDocumento[venta.estado] ?? 'Vigente'}</span></td><td data-label="Cliente">{venta.cliente}</td><td data-label="Base" className="numeric amount-cell">{money(venta.subtotal)}</td><td data-label="IGV" className="numeric amount-cell">{money(venta.igv)}</td><td data-label="Total" className="numeric amount-cell total-emphasis">{money(venta.total)}<small>{venta.formaPago}</small></td><td data-label="Contabilización"><span className={`badge ${venta.asientoContableId ? 'success' : 'warning'}`}>{venta.asientoContableId ? `Asiento #${venta.asientoContableId}` : 'Sin referencia'}</span></td><td data-label="Acciones"><CorreccionesDocumentoButton key={`${empresaId}-venta-${venta.id}`} empresaId={empresaId} tipo="ventas" documentoId={venta.id} onSaved={doc => setVentas(items => items.map(item => item.id === doc.documentoId ? { ...item, estado: doc.estado, subtotalRevertido: doc.historial.reduce((sum, h) => sum + h.subtotal, 0), igvRevertido: doc.historial.reduce((sum, h) => sum + h.igv, 0) } : item))} /></td></tr>)}</tbody></table></div>}
    </section>
  </div>;
}

