import { useEffect, useState } from 'react';
import { cambiarEstadoCuenta, createCuenta, getCuentas, type Cuenta } from '../api/contabilidad';
import { useAppSession } from '../context/AppSessionContext';

const tipos = ['ACTIVO', 'PASIVO', 'PATRIMONIO', 'INGRESO', 'GASTO'];

export function PlanContablePage() {
  const { empresa } = useAppSession();
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [codigo, setCodigo] = useState(''); const [nombre, setNombre] = useState(''); const [tipo, setTipo] = useState('ACTIVO');
  const [message, setMessage] = useState<string | null>(null); const [saving, setSaving] = useState(false); const [busyId, setBusyId] = useState<number | null>(null);
  const load = async () => { if (!empresa) return; try { setCuentas(await getCuentas(empresa.id)); } catch (error) { setMessage(error instanceof Error ? error.message : 'No se pudo cargar el plan contable.'); } };
  useEffect(() => { void load(); }, [empresa?.id]);
  const save = async () => {
    if (!empresa) return; setSaving(true); setMessage(null);
    try { const created = await createCuenta(empresa.id, { codigo, nombre, tipo }); setCuentas((items) => [...items, created].sort((a, b) => a.codigo.localeCompare(b.codigo))); setCodigo(''); setNombre(''); setMessage('Cuenta agregada al plan de esta empresa.'); }
    catch (error) { setMessage(error instanceof Error ? error.message : 'No se pudo crear la cuenta.'); }
    finally { setSaving(false); }
  };
  const toggle = async (cuenta: Cuenta) => {
    if (!empresa || !confirm(`${cuenta.activa ? 'Desactivar' : 'Activar'} la cuenta ${cuenta.codigo}?`)) return;
    setBusyId(cuenta.id); setMessage(null);
    try { const updated = await cambiarEstadoCuenta(empresa.id, cuenta.id, !cuenta.activa); setCuentas((items) => items.map((item) => item.id === updated.id ? updated : item)); }
    catch (error) { setMessage(error instanceof Error ? error.message : 'No se pudo actualizar la cuenta.'); }
    finally { setBusyId(null); }
  };
  return <div className="page-stack"><div className="page-header"><div><span className="eyebrow">Configuración contable</span><h1>Plan contable</h1><p>Este plan es exclusivo de {empresa?.razonSocial}. Las cuentas de otra empresa nunca pueden usarse en sus operaciones.</p></div></div>{message ? <div className={message.includes('agregada') ? 'success-box' : 'error-box'}>{message}</div> : null}<section className="panel form-panel"><div className="panel-head"><div><h2>Nueva cuenta</h2><p className="form-hint">Agrega solo cuentas que correspondan a esta empresa. Las cuentas con movimientos no podrán desactivarse.</p></div></div><div className="form-grid"><label><span>Código</span><input value={codigo} maxLength={20} placeholder="Ej. 6311" onChange={(event) => setCodigo(event.target.value)} /></label><label><span>Tipo</span><select value={tipo} onChange={(event) => setTipo(event.target.value)}>{tipos.map((item) => <option key={item}>{item}</option>)}</select></label><label className="span-2"><span>Nombre de la cuenta</span><input value={nombre} maxLength={200} placeholder="Ej. Energía eléctrica" onChange={(event) => setNombre(event.target.value)} /></label></div><div className="form-actions"><button className="primary-button" disabled={saving || !codigo.trim() || !nombre.trim()} onClick={() => void save()}>{saving ? 'Guardando...' : 'Agregar cuenta'}</button></div></section><section className="panel table-panel"><div className="panel-head journal-list-head"><div><h2>Plan de {empresa?.razonSocial}</h2><p className="form-hint">{cuentas.length} cuentas configuradas</p></div></div><div className="table-wrap"><table><thead><tr><th>Código</th><th>Cuenta</th><th>Tipo</th><th>Estado</th><th /></tr></thead><tbody>{cuentas.map((cuenta) => <tr key={cuenta.id}><td><strong>{cuenta.codigo}</strong></td><td>{cuenta.nombre}</td><td><span className="badge muted">{cuenta.tipo}</span></td><td><span className={cuenta.activa ? 'badge success' : 'badge muted'}>{cuenta.activa ? 'Activa' : 'Inactiva'}</span></td><td><button className="text-button" disabled={busyId === cuenta.id} onClick={() => void toggle(cuenta)}>{busyId === cuenta.id ? 'Procesando...' : cuenta.activa ? 'Desactivar' : 'Activar'}</button></td></tr>)}</tbody></table></div></section></div>;
}
