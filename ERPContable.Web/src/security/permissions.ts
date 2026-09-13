import type { RolEmpresa } from '../api/auth';

export type Modulo = 'compras' | 'ventas' | 'caja' | 'bancos' | 'activos' | 'inventario' | 'contabilidad' | 'ajustes' | 'reportes' | 'usuarios' | 'cierres' | 'configuracion' | 'catalogo';

const permisos: Record<Modulo, RolEmpresa[]> = {
  compras: ['Administrador', 'Contador'],
  ventas: ['Administrador', 'Contador'],
  caja: ['Administrador', 'Contador', 'Cajero'],
  bancos: ['Administrador', 'Contador', 'Cajero'],
  activos: ['Administrador', 'Contador'],
  inventario: ['Administrador', 'Contador'],
  contabilidad: ['Administrador', 'Contador'],
  ajustes: ['Administrador', 'Contador'],
  reportes: ['Administrador', 'Contador', 'Gerente', 'Auditor'],
  usuarios: ['Administrador'],
  cierres: ['Administrador'],
  configuracion: ['Administrador'],
  catalogo: ['Administrador', 'Contador'],
};

export const puedeAcceder = (rol: RolEmpresa | null, modulo: Modulo) => rol !== null && permisos[modulo].includes(rol);
