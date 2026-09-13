import { useEffect, useState } from 'react';
import { cerrarEjercicio, cerrarPeriodo, getCierres, reabrirEjercicio, reabrirPeriodo, type EstadoCierres } from '../api/cierres';
import { getConfiguracionContable, saveConfiguracionContable, type ConfiguracionContable } from '../api/configuracionContable';
import { getCuentas, type Cuenta } from '../api/contabilidad';
import { useAppSession } from '../context/AppSessionContext';

const meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

export function CierresPage() {
  const { empresa, ejercicio } = useAppSession();
  const [estado, setEstado] = useState<EstadoCierres | null>(null);
  const [config, setConfig] = useState<ConfiguracionContable | null>(null);
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [cuentaResultadoId, setCuentaResultadoId] = useState('');
  const [cuentaInventarioId, setCuentaInventarioId] = useState('');
  const [cuentaCostoVentasId, setCuentaCostoVentasId] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState<string | null>(null);
  const year = ejercicio ?? new Date().getFullYear();
  const cuentasPatrimonio = cuentas.filter((item) => item.activa && item.tipo === 'PATRIMONIO');
  const cuentasActivo = cuentas.filter((item) => item.activa && item.tipo === 'ACTIVO');
  const cuentasGasto = cuentas.filter((item) => item.activa && item.tipo === 'GASTO');

  const load = async () => {
    if (!empresa) return;
    try {
      const [cierres, configuracion, plan] = await Promise.all([getCierres(empresa.id, year), getConfiguracionContable(empresa.id), getCuentas(empresa.id)]);
      setEstado(cierres); setConfig(configuracion); setCuentas(plan);
      setCuentaResultadoId(configuracion.cuentaResultadoAcumuladoId?.toString() ?? '');
      setCuentaInventarioId(configuracion.cuentaInventarioId?.toString() ?? '');
      setCuentaCostoVentasId(configuracion.cuentaCostoVentasId?.toString() ?? '');
    } catch (err) { setError(err instanceof Error ? err.message : 'No se pudieron cargar los cierres.'); }
  };

  useEffect(() => { void load(); }, [empresa?.id, year]);

  const changePeriod = async (mes: number, cerrado: boolean) => {
    if (!empresa || !confirm(`${cerrado ? 'Cerrar' : 'Reabrir'} ${meses[mes - 1]} de ${year}?`)) return;
    setBusy(`p-${mes}`); setError(null);
    try { if (cerrado) await cerrarPeriodo(empresa.id, year, mes); else await reabrirPeriodo(empresa.id, year, mes); await load(); }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo actualizar el período.'); }
    finally { setBusy(null); }
  };

  const saveConfig = async () => {
    if (!empresa || !cuentaResultadoId || !cuentaInventarioId || !cuentaCostoVentasId) return;
    setBusy('config'); setError(null);
    try {
      const updated = await saveConfiguracionContable(empresa.id, {
        cuentaResultadoAcumuladoId: Number(cuentaResultadoId),
        cuentaInventarioId: Number(cuentaInventarioId),
        cuentaCostoVentasId: Number(cuentaCostoVentasId),
      });
      setConfig(updated);
    }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo guardar la configuración.'); }
    finally { setBusy(null); }
  };

  const changeExercise = async (cerrado: boolean) => {
    if (!empresa || !confirm(cerrado ? `Al cerrar ${year} se crearán los asientos automáticos de cierre y apertura. ¿Deseas continuar?` : `¿Reabrir el ejercicio ${year}? Se eliminarán sus asientos automáticos si no hay movimientos posteriores.`)) return;
    setBusy('ejercicio'); setError(null);
    try { if (cerrado) await cerrarEjercicio(empresa.id, year); else await reabrirEjercicio(empresa.id, year); await load(); }
    catch (err) { setError(err instanceof Error ? err.message : 'No se pudo actualizar el ejercicio.'); }
    finally { setBusy(null); }
  };

  const allClosed = estado?.periodos.every((item) => item.cerrado) ?? false;
  const ejercicioCerrado = estado?.cierreEjercicio?.cerrado ?? false;

  return <div className="page-stack">
    <div className="page-header"><div><span className="eyebrow">Control contable</span><h1>Cierres del ejercicio {year}</h1><p>El cierre anual transfiere automáticamente los ingresos y gastos a resultados acumulados y crea la apertura del siguiente ejercicio.</p></div></div>
    {error ? <div className="error-box">{error}</div> : null}
    <section className="panel form-panel"><div className="panel-head"><div><span className="eyebrow">Configuración requerida</span><h2>Cuentas automáticas</h2><p>Estas cuentas se usarán al registrar compras, ventas y cierres de la empresa.</p></div></div><div className="form-grid"><label><span>Resultados acumulados <small>(patrimonio)</small></span><select value={cuentaResultadoId} disabled={ejercicioCerrado || busy === 'config'} onChange={(event) => setCuentaResultadoId(event.target.value)}><option value="">Seleccione una cuenta</option>{cuentasPatrimonio.map((cuenta) => <option value={cuenta.id} key={cuenta.id}>{cuenta.codigo} - {cuenta.nombre}</option>)}</select></label><label><span>Inventario <small>(activo)</small></span><select value={cuentaInventarioId} disabled={ejercicioCerrado || busy === 'config'} onChange={(event) => setCuentaInventarioId(event.target.value)}><option value="">Seleccione una cuenta</option>{cuentasActivo.map((cuenta) => <option value={cuenta.id} key={cuenta.id}>{cuenta.codigo} - {cuenta.nombre}</option>)}</select></label><label><span>Costo de ventas <small>(gasto)</small></span><select value={cuentaCostoVentasId} disabled={ejercicioCerrado || busy === 'config'} onChange={(event) => setCuentaCostoVentasId(event.target.value)}><option value="">Seleccione una cuenta</option>{cuentasGasto.map((cuenta) => <option value={cuenta.id} key={cuenta.id}>{cuenta.codigo} - {cuenta.nombre}</option>)}</select></label><div><p className="form-hint">El sistema valida que las cuentas estén activas, pertenezcan a esta empresa y sean de la naturaleza correcta.</p><button className="secondary-button" disabled={!cuentaResultadoId || !cuentaInventarioId || !cuentaCostoVentasId || ejercicioCerrado || busy === 'config'} onClick={() => void saveConfig()}>{busy === 'config' ? 'Guardando...' : 'Guardar configuración'}</button></div></div></section>
    <section className="panel close-summary"><div><span>Empresa</span><strong>{empresa?.razonSocial}</strong></div><div><span>Asientos del ejercicio</span><strong>{estado?.asientosEjercicio ?? '...'}</strong></div><div><span>Estado del ejercicio</span><strong className={ejercicioCerrado ? 'close-state closed' : 'close-state'}>{ejercicioCerrado ? 'Cerrado' : 'Abierto'}</strong></div><button className={ejercicioCerrado ? 'secondary-button' : 'primary-button'} disabled={busy === 'ejercicio' || (!ejercicioCerrado && (!allClosed || !config?.cuentaResultadoAcumuladoId || !config?.cuentaInventarioId || !config?.cuentaCostoVentasId))} onClick={() => void changeExercise(!ejercicioCerrado)}>{busy === 'ejercicio' ? 'Procesando...' : ejercicioCerrado ? 'Reabrir ejercicio' : 'Cerrar ejercicio'}</button></section>
    {ejercicioCerrado ? <section className="panel close-summary"><div><span>Asiento de cierre</span><strong>{estado?.cierreEjercicio?.asientoCierreId ? `#${estado.cierreEjercicio.asientoCierreId}` : 'Sin saldos de resultados'}</strong></div><div><span>Asiento de apertura {year + 1}</span><strong>{estado?.cierreEjercicio?.asientoAperturaId ? `#${estado.cierreEjercicio.asientoAperturaId}` : 'Sin saldos de balance'}</strong></div><div><span>Cuenta configurada</span><strong>{config?.cuentaResultadoAcumulado ?? '-'}</strong></div></section> : null}
    <section className="close-grid">{estado?.periodos.map((item) => <article className={`panel close-card ${item.cerrado ? 'is-closed' : ''}`} key={item.mes}><span>{String(item.mes).padStart(2, '0')}</span><strong>{meses[item.mes - 1]}</strong><p>{item.cerrado ? 'Período bloqueado' : 'Período disponible'}</p><button className={item.cerrado ? 'secondary-button' : 'primary-button'} disabled={busy === `p-${item.mes}` || ejercicioCerrado} onClick={() => void changePeriod(item.mes, !item.cerrado)}>{busy === `p-${item.mes}` ? 'Procesando...' : item.cerrado ? 'Reabrir' : 'Cerrar'}</button></article>)}</section>
    <p className="form-hint">Para cerrar el ejercicio deben estar cerrados los doce meses y configuradas las tres cuentas automáticas. No se podrá reabrir si existen movimientos posteriores al ejercicio.</p>
  </div>;
}

