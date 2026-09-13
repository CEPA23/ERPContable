import { useEffect, useRef, useState } from 'react';
import { estadosDocumento, getCorrecciones, registrarCorreccion, type CorreccionDocumento, type CorreccionRequest } from '../api/correcciones';
import './CorreccionesDocumento.css';

type Props = { empresaId: number; tipo: 'compras' | 'ventas'; documentoId: number; onSaved: (doc: CorreccionDocumento) => void };
const money = (value: number) => value.toLocaleString('es-PE', { style: 'currency', currency: 'PEN' });
const localDate = () => new Date().toLocaleDateString('en-CA');

export function CorreccionesDocumentoButton(props: Props) {
  const [open, setOpen] = useState(false);
  return <><button type="button" className="text-button" onClick={() => setOpen(true)}>Devolver / anular</button>
    {open && <CorreccionesDocumento {...props} onClose={() => setOpen(false)} />}</>;
}

function CorreccionesDocumento({ empresaId, tipo, documentoId, onSaved, onClose }: Props & { onClose: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null);
  const [doc, setDoc] = useState<CorreccionDocumento | null>(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [mode, setMode] = useState<CorreccionRequest['tipo']>('DEVOLUCION');
  const [fecha, setFecha] = useState(localDate);
  const [motivo, setMotivo] = useState('');
  const [cantidades, setCantidades] = useState<Record<number, string>>({});
  const [confirmed, setConfirmed] = useState(false);
  const [reload, setReload] = useState(0);
  const pending = useRef<CorreccionRequest | null>(null);

  useEffect(() => {
    dialog.current?.showModal();
  }, []);
  useEffect(() => {
    let active = true;
    setLoading(true); setError('');
    void getCorrecciones(empresaId, tipo, documentoId).then(result => {
      if (active) { setDoc(result); onSaved(result); }
    }).catch(err => { if (active) setError(err instanceof Error ? err.message : 'No se pudo cargar el documento.'); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
    // onSaved refreshes the parent summary and must not trigger another fetch.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [empresaId, tipo, documentoId, reload]);

  const closed = doc?.estado === 'ANULADO' || doc?.estado === 'DEVUELTO';
  const seleccion = doc?.detalles.filter(d => Number(cantidades[d.id] || 0) > 0).map(d => ({ detalleId: d.id, cantidad: Number(cantidades[d.id]) })) ?? [];
  const changed = () => { setConfirmed(false); setSuccess(''); };
  const save = async (event: React.FormEvent) => {
    event.preventDefault();
    if (saving || !confirmed || !doc) return;
    const payload = { tipo: mode, fecha, motivo: motivo.trim(), detalles: mode === 'ANULACION' ? [] : seleccion };
    // Reuse the same key on retry; the server stores it in the same transaction as the reversal.
    if (!pending.current || JSON.stringify({ ...pending.current, solicitudId: undefined }) !== JSON.stringify(payload))
      pending.current = { ...payload, solicitudId: crypto.randomUUID() };
    setSaving(true); setError(''); setSuccess('');
    try {
      const updated = await registrarCorreccion(empresaId, tipo, documentoId, pending.current);
      setDoc(updated); onSaved(updated); setCantidades({}); setMotivo(''); setConfirmed(false); pending.current = null;
      setSuccess(mode === 'ANULACION' ? 'Documento anulado. Las reversiones quedaron registradas.' : 'Devolución registrada. Revisa sus asientos y movimientos en el historial.');
    } catch (err) { setError(err instanceof Error ? err.message : 'No se pudo registrar la corrección.'); }
    finally { setSaving(false); }
  };

  return <dialog ref={dialog} className="correction-dialog" aria-labelledby="correction-title" onCancel={e => { if (saving) e.preventDefault(); }} onClose={onClose}>
    <div className="correction-heading"><div><h2 id="correction-title">Devoluciones y anulaciones</h2><p>{doc?.comprobante ?? `Documento #${documentoId}`} · {tipo === 'ventas' ? 'Venta' : 'Compra'}</p></div>
      <button type="button" className="secondary-button" autoFocus disabled={saving} onClick={() => dialog.current?.close()}>Cerrar</button></div>
    <div aria-live="polite">{loading && <p role="status">Cargando documento e historial…</p>}{error && <div className="error-box" role="alert">{error} <button type="button" className="text-button" disabled={saving} onClick={() => setReload(x => x + 1)}>Actualizar documento</button></div>}{success && <p className="success-box" role="status">{success}</p>}</div>
    {doc && !loading && <>
      <div className="correction-summary"><span className="badge">{estadosDocumento[doc.estado]}</span><span>Original: <strong>{money(doc.totalOriginal)}</strong></span><span>Revertido: <strong>{money(doc.totalRevertido)}</strong></span><span>Neto: <strong>{money(doc.totalOriginal - doc.totalRevertido)}</strong></span></div>
      {!closed && <form onSubmit={e => void save(e)}>
        <fieldset disabled={saving} className="correction-fields">
          <legend>Registrar una corrección</legend>
          <div className="correction-form-grid"><label>Acción<select value={mode} onChange={e => { setMode(e.target.value as CorreccionRequest['tipo']); changed(); }}><option value="DEVOLUCION">Devolver cantidades</option><option value="ANULACION" disabled={doc.historial.length > 0}>Anular todo el documento</option></select></label>
            <label>Fecha de la corrección<input type="date" required min={doc.fecha.slice(0, 10)} value={fecha} onChange={e => { setFecha(e.target.value); changed(); }} /></label></div>
          <p className="form-hint">{mode === 'ANULACION' ? 'Se revertirá todo el documento. Su período original y el período de la corrección deben estar abiertos.' : 'Indica las cantidades que deseas devolver. La fecha debe corresponder a un período abierto.'} {tipo === 'ventas' ? 'Los productos volverán al inventario con su costo original.' : 'Los productos saldrán del inventario; debe existir stock y valor suficientes.'}</p>
          {mode === 'DEVOLUCION' && <>
            <button type="button" className="text-button" onClick={() => { setCantidades(Object.fromEntries(doc.detalles.map(d => [d.id, String(d.disponible)]))); changed(); }}>Seleccionar todo lo pendiente</button>
            <div className="correction-lines">{doc.detalles.map(d => <label key={d.id} className="correction-line"><span><strong>{d.descripcion}</strong><small>Original: {d.cantidad} · Devuelto: {d.devuelta} · Disponible: {d.disponible}</small></span><span>Cantidad a devolver<input aria-label={`Devolver ${d.descripcion}`} type="number" min="0" max={d.disponible} step="0.0001" disabled={!d.disponible} value={cantidades[d.id] ?? ''} placeholder="0" onChange={e => { setCantidades(values => ({ ...values, [d.id]: e.target.value })); changed(); }} /></span></label>)}</div>
          </>}
          <label className="correction-reason">Motivo de la corrección<textarea required maxLength={500} rows={3} value={motivo} onChange={e => { setMotivo(e.target.value); changed(); }} placeholder="Ejemplo: devolución de productos por pedido incorrecto" /></label>
          <label className="correction-confirm"><input type="checkbox" checked={confirmed} onChange={e => setConfirmed(e.target.checked)} required /><span>He revisado el documento, la fecha y las cantidades. Confirmo registrar la corrección y sus asientos.</span></label>
          <div className="form-actions"><button type="submit" className={mode === 'ANULACION' ? 'secondary-button correction-danger' : 'primary-button'} disabled={!confirmed || !motivo.trim() || (mode === 'DEVOLUCION' && !seleccion.length)}>{saving ? 'Registrando…' : mode === 'ANULACION' ? 'Confirmar anulación' : 'Registrar devolución'}</button></div>
        </fieldset>
      </form>}
      <section className="correction-history"><h3>Historial de correcciones</h3>{!doc.historial.length && <p className="form-hint">Este documento todavía no tiene correcciones.</p>}
        {doc.historial.map(h => <article key={h.id}><strong>{h.tipo === 'ANULACION' ? 'Anulación' : 'Devolución'} #{h.id} · {money(h.subtotal + h.igv)}</strong><p>{h.fecha.slice(0, 10)} · {h.usuario} · Registrado: {new Date(h.creadoEnUtc).toLocaleString('es-PE')}</p><p>{h.motivo}</p><p>Asiento: {h.asientoContableId ? `#${h.asientoContableId}` : 'Sin importe contable'}{h.asientoCostoId ? ` · Costo de ventas: #${h.asientoCostoId}` : ''}</p>
          <ul>{h.detalles.map(d => <li key={d.detalleId}>{doc.detalles.find(l => l.id === d.detalleId)?.descripcion} · {d.cantidad} unidades{d.movimientoInventarioId ? ` · Movimiento #${d.movimientoInventarioId}` : ''}</li>)}</ul></article>)}
      </section>
    </>}
  </dialog>;
}
