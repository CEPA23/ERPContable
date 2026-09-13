import { request } from './client';
export type Cliente = { id: number; documento: string; razonSocial: string; activo: boolean };
export type Venta = { id: number; fecha: string; cliente: string; comprobante: string; subtotal: number; igv: number; total: number; formaPago: string; estado: string; subtotalRevertido: number; igvRevertido: number; asientoContableId?: number; asientoCostoVentaId?: number | null; detalles?: { cuenta: string; descripcion: string; cantidad: number; precioUnitario: number; total: number; productoServicioId?: number | null; producto?: string | null }[] };
export type VentaCreate = { fecha: string; clienteId: number; tipoComprobante: string; serie: string; numero: string; igv: number; formaPago: string; cuentaContrapartidaId: number; cuentaIgvId: number; detalles: { cuentaContableId: number; descripcion: string; cantidad: number; precioUnitario: number; productoServicioId?: number | null }[] };
export const getClientes = (empresaId: number) => request<Cliente[]>(`/empresas/${empresaId}/ventas/clientes`);
export const createCliente = (empresaId: number, payload: { documento: string; razonSocial: string }) => request<Cliente>(`/empresas/${empresaId}/ventas/clientes`, { method: 'POST', body: JSON.stringify(payload) });
export const getVentas = (empresaId: number) => request<Venta[]>(`/empresas/${empresaId}/ventas`);
export const createVenta = (empresaId: number, payload: VentaCreate) => request<Venta>(`/empresas/${empresaId}/ventas`, { method: 'POST', body: JSON.stringify(payload) });
