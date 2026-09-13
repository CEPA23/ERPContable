import { Link } from 'react-router-dom';
import { useAppSession } from '../context/AppSessionContext';
import { puedeAcceder, type Modulo } from '../security/permissions';

const operaciones: { key: string; modulo: Modulo; label: string; description: string; to: string }[] = [
  { key: 'compras', modulo: 'compras', label: 'Compras', description: 'Registro de adquisiciones y proveedores.', to: '/operaciones/compras' },
  { key: 'ventas', modulo: 'ventas', label: 'Ventas', description: 'Registro de ventas y clientes.', to: '/operaciones/ventas' },
  { key: 'caja', modulo: 'caja', label: 'Caja', description: 'Ingresos y egresos de efectivo.', to: '/operaciones/caja' },
  { key: 'bancos', modulo: 'bancos', label: 'Bancos', description: 'Movimientos y conciliación bancaria.', to: '/operaciones/bancos' },
  { key: 'diario', modulo: 'contabilidad', label: 'Diario manual', description: 'Asientos contables manuales.', to: '/contabilidad/asientos' },
  { key: 'activos', modulo: 'activos', label: 'Activos', description: 'Control y depreciación de activos.', to: '/operaciones/activos' },
  { key: 'ajustes', modulo: 'ajustes', label: 'Ajustes', description: 'Ajustes y regularizaciones del período.', to: '/operaciones/ajustes' },
  { key: 'inventario', modulo: 'inventario', label: 'Inventario', description: 'Existencias, entradas, salidas y kardex.', to: '/operaciones/inventario' },
];

const iconos: Record<string, string> = { compras: 'carrito-de-compras.svg', ventas: 'carrito-de-compras.svg', caja: 'lista.svg', bancos: 'grafico-histograma.svg', diario: 'lista.svg', activos: 'editar.svg', ajustes: 'actualizar.svg', inventario: 'lista.svg' };

function OperationCard({ icon, title, description, action, to }: { icon: string; title: string; description: string; action: string; to: string }) {
  return <Link className="operation-card panel" to={to}><img className="operation-icon" src={`/assets/${icon}`} alt="" /><strong>{title}</strong><span>{description}</span><b>{action} →</b></Link>;
}

export function OperacionesPage() {
  const { rol } = useAppSession();
  const visibles = operaciones.filter((item) => puedeAcceder(rol, item.modulo));
  return <div className="page-stack"><div className="page-header"><div><span className="eyebrow">Operaciones diarias</span><h1>Registrar operaciones</h1><p>Cada operación genera su asiento y alimenta automáticamente el motor contable.</p></div></div><div className="operation-grid">
    {puedeAcceder(rol, 'compras') ? <OperationCard icon="usuarios-alt.svg" title="Proveedores" description="Maestro de proveedores para compras." action="Configurar" to="/operaciones/proveedores" /> : null}
    {puedeAcceder(rol, 'ventas') ? <OperationCard icon="usuarios-alt.svg" title="Clientes" description="Maestro de clientes para ventas." action="Configurar" to="/operaciones/clientes" /> : null}
    {visibles.map((item) => <OperationCard key={item.key} icon={iconos[item.key]} title={item.label} description={item.description} action="Registrar" to={item.to} />)}
  </div></div>;
}
