import { useState } from 'react';
import { downloadReporte, getReporte, reportes, type ReporteKey } from '../api/reportes';
import { useAppSession } from '../context/AppSessionContext';
import { ReportDataPreview } from '../components/ReportDataPreview';

const localToday = () => { const date = new Date(); const offset = date.getTimezoneOffset(); return new Date(date.getTime() - offset * 60000).toISOString().slice(0, 10); };

export function ReportesPage() {
  const today = localToday();
  const { empresa, ejercicio, periodo } = useAppSession();
  const empresaId = empresa?.id ?? 0;
  const [reporte, setReporte] = useState<ReporteKey>('libro-diario');
  const [desde, setDesde] = useState(`${today.slice(0, 4)}-01-01`);
  const [hasta, setHasta] = useState(today);
  const [resultado, setResultado] = useState<unknown>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [downloadLoading, setDownloadLoading] = useState<'excel' | 'pdf' | null>(null);

  const consultar = async () => {
    setMessage(null);
    if (desde > hasta) { setMessage('La fecha inicial no puede ser posterior a la fecha final.'); return; }
    setLoading(true);
    try { if (!empresaId) throw new Error('Seleccione una empresa.'); setResultado(await getReporte(empresaId, reporte, desde, hasta)); }
    catch (error) { setResultado(null); setMessage(error instanceof Error ? error.message : 'No se pudo consultar el reporte.'); }
    finally { setLoading(false); }
  };

  const descargar = async (formato: 'excel' | 'pdf') => {
    setMessage(null); setDownloadLoading(formato);
    try { if (!empresaId) throw new Error('Seleccione una empresa.'); await downloadReporte(empresaId, reporte, formato, desde, hasta); setMessage(`Reporte descargado en formato ${formato === 'excel' ? 'Excel' : 'PDF'}.`); }
    catch (error) { setMessage(error instanceof Error ? error.message : 'No se pudo descargar el reporte.'); }
    finally { setDownloadLoading(null); }
  };

  const reportLabel = reportes.find((item) => item.key === reporte)?.label ?? reporte;
  const busy = loading || downloadLoading !== null;

  return <div className="page-stack reports-page">
    <section className="page-header report-page-header"><div><span className="eyebrow">Consulta financiera</span><h1>Reportes contables</h1><p>Consulta la información generada por los asientos de la empresa activa y descarga el resultado.</p></div><div className="transaction-context"><span>Empresa y período</span><strong>{empresa?.razonSocial ?? 'Sin empresa seleccionada'}</strong><span className="transaction-company">Ejercicio {ejercicio ?? '—'} · {periodo ? `Mes ${String(periodo).padStart(2, '0')}` : 'Período no seleccionado'}</span></div></section>

    <section className="panel form-panel report-filter"><div><span className="eyebrow">Selección del reporte</span><h2>Define el alcance de la consulta</h2><p className="form-hint">El período se envía directamente al reporte seleccionado.</p></div><div className="report-types">{reportes.map((item) => <button type="button" key={item.key} className={`report-type ${reporte === item.key ? 'selected' : ''}`} onClick={() => { setReporte(item.key); setResultado(null); setMessage(null); }} aria-pressed={reporte === item.key}>{item.label}</button>)}</div><div className="report-dates"><label><span>Desde</span><input type="date" value={desde} onChange={(event) => setDesde(event.target.value)} /></label><label><span>Hasta</span><input type="date" value={hasta} onChange={(event) => setHasta(event.target.value)} /></label></div><div className="form-actions report-actions"><button className="primary-button" type="button" onClick={() => void consultar()} disabled={busy}>{loading ? 'Consultando...' : 'Consultar reporte'}</button><span className="action-divider" aria-hidden="true" /><button className="secondary-button" type="button" onClick={() => void descargar('excel')} disabled={busy}>{downloadLoading === 'excel' ? 'Descargando...' : 'Descargar Excel'}</button><button className="secondary-button" type="button" onClick={() => void descargar('pdf')} disabled={busy}>{downloadLoading === 'pdf' ? 'Descargando...' : 'Descargar PDF'}</button></div>{message ? <div className={message.startsWith('Reporte descargado') ? 'success-box' : 'error-box'} role="status">{message}</div> : null}</section>

    <section className="panel table-panel report-result" aria-live="polite"><div className="panel-head report-result-head"><div><span className="eyebrow">Vista previa</span><h2>{reportLabel}</h2><p className="form-hint">{resultado ? `Datos recibidos para ${desde} — ${hasta}.` : 'La respuesta aparecerá aquí después de consultar.'}</p></div>{loading ? <span className="badge muted">Generando reporte...</span> : resultado ? <span className="badge success">Consulta completada</span> : null}</div>{resultado ? <ReportDataPreview data={resultado} /> : <div className="empty-state">Selecciona un tipo de reporte y un período para consultar.</div>}</section>
  </div>;
}

