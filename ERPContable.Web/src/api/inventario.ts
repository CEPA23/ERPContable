import { request } from './client';

export type InventarioProducto = { id: number; productoId: number; codigo: string; producto: string; unidad: string; stockActual: number; stockMinimo: number; costoPromedio: number; bajoMinimo: boolean };
export type MovimientoInventario = { id: number; productoId: number; codigo: string; producto: string; tipo: 'ENTRADA' | 'SALIDA' | 'AJUSTE'; cantidad: number; costoUnitario: number; stockAnterior: number; stockPosterior: number; referencia?: string | null; observacion?: string | null; fechaUtc: string };
export type Inventario = { productos: InventarioProducto[]; movimientos: MovimientoInventario[] };

const base = (empresaId: number) => `/empresas/${empresaId}/inventario`;
export const getInventario = (empresaId: number, productoId?: number) => request<Inventario>(`${base(empresaId)}${productoId ? `?productoId=${productoId}` : ''}`);
export const createMovimiento = (empresaId: number, payload: { productoId: number; tipo: string; cantidad: number; stockObjetivo?: number | null; costoUnitario: number; referencia?: string; observacion?: string; fecha?: string }) => request<MovimientoInventario>(`${base(empresaId)}/movimientos`, { method: 'POST', body: JSON.stringify(payload) });
export const updateStockMinimo = (empresaId: number, productoId: number, stockMinimo: number) => request<InventarioProducto>(`${base(empresaId)}/productos/${productoId}/stock-minimo`, { method: 'PUT', body: JSON.stringify({ stockMinimo }) });
