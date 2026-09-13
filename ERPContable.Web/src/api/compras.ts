import { request } from './client';
export type Proveedor = { id: number; documento: string; razonSocial: string; direccion?: string; email?: string; activo: boolean };
export type Compra = { id: number; fecha: string; proveedor: string; comprobante: string; subtotal: number; igv: number; total: number; formaPago: string; estado: string; subtotalRevertido: number; igvRevertido: number; asientoContableId?: number; detalles: { cuenta: string; descripcion: string; cantidad: number; precioUnitario: number; total: number; productoServicioId?: number | null; producto?: string | null }[] };
export type CompraCreate = { fecha: string; proveedorId: number; tipoComprobante: string; serie: string; numero: string; igv: number; formaPago: string; cuentaContrapartidaId: number; cuentaIgvId: number; detalles: { cuentaContableId: number; descripcion: string; cantidad: number; precioUnitario: number; productoServicioId?: number | null }[] };
export const getProveedores = (empresaId: number) => request<Proveedor[]>(`/empresas/${empresaId}/compras/proveedores`);
export const createProveedor = (empresaId: number, payload: { documento: string; razonSocial: string; direccion?: string; email?: string }) => request<Proveedor>(`/empresas/${empresaId}/compras/proveedores`, { method: 'POST', body: JSON.stringify(payload) });
export const getCompras = (empresaId: number) => request<Compra[]>(`/empresas/${empresaId}/compras`);
export const createCompra = (empresaId: number, payload: CompraCreate) => request<Compra>(`/empresas/${empresaId}/compras`, { method: 'POST', body: JSON.stringify(payload) });
