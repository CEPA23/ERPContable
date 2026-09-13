import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { getDashboardResumen, type DashboardResumen } from '../api/dashboard';
import { getEmpresas } from '../api/empresas';
import { useAppSession } from '../context/AppSessionContext';
import { systemStatusLabel, useSystemHealth } from '../hooks/useSystemHealth';
import { puedeAcceder } from '../security/permissions';
import type { Empresa } from '../types/empresa';

const money = (value: number) => new Intl.NumberFormat('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);

export function DashboardPage() {
  const { empresa, ejercicio, periodo, rol } = useAppSession();
  const systemStatus = useSystemHealth();
  const [empresas, setEmpresas] = useState<Empresa[]>([]);
  const [resumen, setResumen] = useState<DashboardResumen | null>(null);
  const [loading, setLoading] = useState(true);
  const [dashboardLoading, setDashboardLoading] = useState(false);
  const [error, setError] = useState(false);
  const [dashboardError, setDashboardError] = useState<string | null>(null);
  const year = ejercicio ?? new Date().getFullYear();
  const month = periodo ?? new Date().getMonth() + 1;

  useEffect(() => {
    let active = true;
    getEmpresas().then((items) => { if (active) setEmpresas(items); }).catch(() => { if (active) setError(true); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  useEffect(() => {
    if (!empresa) { setResumen(null); return; }
    let active = true;
    setDashboardLoading(true); setDashboardError(null);
    getDashboardResumen(empresa.id, year, month).then((data) => { if (active) setResumen(data); }).catch((reason) => { if (active) { setResumen(null); setDashboardError(reason instanceof Error ? reason.message : 'No se pudo cargar el resumen contable.'); } }).finally(() => { if (active) setDashboardLoading(false); });
    return () => { active = false; };
  }, [empresa?.id, year, month]);

  const maxChart = useMemo(() => Math.max(1, ...(resumen?.tendencia.flatMap((item) => [item.ventas, item.compras]) ?? [1])), [resumen]);
  const hasCompany = empresas.length > 0 || Boolean(empresa);
  const empresaCount = loading ? '...' : error ? '—' : String(empresas.length);
  const netoCaja = resumen ? resumen.ingresosCajaMes - resumen.egresosCajaMes : 0;

  return <div className="page-stack dashboard-page">
    <section className="dashboard-heading">
      <div><span className="eyebrow">Centro de trabajo contable</span><h1>Resumen operativo</h1><p>{hasCompany ? `Información del período ${String(month).padStart(2, '0')} de ${year} para ${empresa?.razonSocial ?? 'la empresa activa'}.` : 'Seleccione una empresa para consultar su información contable.'}</p></div>
      <div className="transaction-context dashboard-context"><span>Contexto activo</span><strong>{empresa?.razonSocial ?? 'Sin empresa seleccionada'}</strong><span className="transaction-company">Ejercicio {year} · Mes {String(month).padStart(2, '0')}</span></div>
    </section>

    <section className="dashboard-toolbar"><span className="eyebrow">Accesos de trabajo</span><div className="dashboard-quick-actions">{puedeAcceder(rol, 'contabilidad') ? <Link className="primary-button" to="/contabilidad/asientos">Nuevo asiento</Link> : null}{puedeAcceder(rol, 'compras') ? <Link className="secondary-button" to="/operaciones/compras">Registrar compra</Link> : null}{puedeAcceder(rol, 'ventas') ? <Link className="secondary-button" to="/operaciones/ventas">Registrar venta</Link> : null}{puedeAcceder(rol, 'reportes') ? <Link className="text-button" to="/contabilidad/reportes">Consultar reportes</Link> : null}</div></section>

    {dashboardError ? <div className="error-box" role="alert">{dashboardError}<span className="form-hint"> Verifica la empresa y el período seleccionados.</span></div> : null}
    <section className="stats-grid dashboard-kpis" aria-label="Indicadores del período">
      {[['Ventas del período', resumen?.ventasMes, 'Operaciones de venta'], ['Compras del período', resumen?.comprasMes, 'Operaciones de compra'], ['Ingresos de caja', resumen?.ingresosCajaMes, 'Movimientos registrados'], ['Egresos de caja', resumen?.egresosCajaMes, 'Movimientos registrados']].map(([label, value, hint], index) => <article className="stat-card panel" key={String(label)}><span><i className={`stat-icon ${['blue', 'purple', 'sky', 'amber'][index]}`} />{label}</span><strong>{dashboardLoading ? 'Cargando...' : value === undefined ? '—' : money(Number(value))}</strong><small>{hint} · {year}/{String(month).padStart(2, '0')}</small></article>)}
    </section>

    <section className="dashboard-panels"><article className="panel chart-panel"><div className="panel-head"><div><span className="eyebrow">Evolución disponible</span><h2>Ventas vs. compras</h2><p className="form-hint">Serie entregada por el backend para el contexto actual.</p></div><span className="chart-legend"><i className="legend-sales" />Ventas <i className="legend-purchases" />Compras</span></div>{dashboardLoading ? <div className="empty-state">Cargando tendencia...</div> : resumen?.tendencia.length ? <div className="bar-chart">{resumen.tendencia.map((item) => <div className="bar-group" key={item.etiqueta}><div className="bars"><span className="bar sales" title={`Ventas: ${money(item.ventas)}`} style={{ height: `${Math.max(3, item.ventas / maxChart * 100)}%` }} /><span className="bar purchases" title={`Compras: ${money(item.compras)}`} style={{ height: `${Math.max(3, item.compras / maxChart * 100)}%` }} /></div><small>{item.etiqueta}</small></div>)}</div> : <div className="empty-state">No hay movimientos para graficar en la serie recibida.</div>}</article><article className="panel dashboard-side"><span className="eyebrow">Lectura financiera</span><strong className={netoCaja >= 0 ? 'financial-positive' : 'financial-negative'}>{dashboardLoading ? 'Cargando...' : resumen ? money(netoCaja) : '—'}</strong><p>Movimiento neto de caja del período visible: ingresos menos egresos.</p><hr /><span className="eyebrow">Estado del sistema</span><strong className={`online-label ${systemStatus}`}><i /> {systemStatusLabel(systemStatus)}</strong><p>{systemStatus === 'ok' ? 'API y base de datos disponibles.' : 'Revisa la conexión del servidor.'}</p><hr /><span className="eyebrow">Empresas registradas</span><strong>{empresaCount} {empresas.length === 1 ? 'empresa activa' : 'empresas administradas'}</strong>{puedeAcceder(rol, 'cierres') ? <Link className="secondary-button dashboard-link" to="/contabilidad/cierres">Gestionar cierres</Link> : null}</article></section>
  </div>;
}

