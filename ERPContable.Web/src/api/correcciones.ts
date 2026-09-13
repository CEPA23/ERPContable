import { request } from './client';

export type CorreccionDocumento = {
  documentoId: number; comprobante: string; fecha: string; estado: string;
  totalOriginal: number; totalRevertido: number;
  detalles: { id: number; descripcion: string; cantidad: number; devuelta: number; disponible: number; precioUnitario: number }[];
  historial: {
    id: number; tipo: string; fecha: string; motivo: string; usuario: string; creadoEnUtc: string;
    subtotal: number; igv: number; asientoContableId: number | null; asientoCostoId: number | null;
    detalles: { detalleId: number; cantidad: number; subtotal: number; costoInventario: number; movimientoInventarioId: number | null }[];
  }[];
};
export type CorreccionRequest = {
  solicitudId: string; tipo: 'DEVOLUCION' | 'ANULACION'; fecha: string; motivo: string;
  detalles: { detalleId: number; cantidad: number }[];
};
export const estadosDocumento: Record<string, string> = {
  VIGENTE: 'Vigente', ANULADO: 'Anulado', DEVUELTO: 'Devuelto totalmente', DEVOLUCION_PARCIAL: 'Devolución parcial',
};
export const getCorrecciones = (empresa: number, tipo: 'compras' | 'ventas', id: number) =>
  request<CorreccionDocumento>(`/empresas/${empresa}/${tipo}/${id}/correcciones`);
export const registrarCorreccion = (empresa: number, tipo: 'compras' | 'ventas', id: number, payload: CorreccionRequest) =>
  request<CorreccionDocumento>(`/empresas/${empresa}/${tipo}/${id}/correcciones`, { method: 'POST', body: JSON.stringify(payload) });
