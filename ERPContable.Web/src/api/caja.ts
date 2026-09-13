import { request } from './client';
export type MovimientoCaja = { id: number; fecha: string; tipo: string; descripcion: string; monto: number; cuentaCaja: string; cuentaContrapartida: string; asientoContableId?: number };
export const getMovimientosCaja = (empresaId: number) => request<MovimientoCaja[]>(`/empresas/${empresaId}/caja`);
export const createMovimientoCaja = (empresaId: number, payload: { fecha: string; tipo: string; descripcion: string; monto: number; cuentaCajaId: number; cuentaContrapartidaId: number }) => request<MovimientoCaja>(`/empresas/${empresaId}/caja`, { method: 'POST', body: JSON.stringify(payload) });
