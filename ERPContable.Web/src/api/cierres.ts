import { request } from './client';

export type CierrePeriodo = { id: number; ejercicio: number; mes: number; cerrado: boolean; cerradoEnUtc?: string | null; reabiertoEnUtc?: string | null; observacion?: string | null };
export type CierreEjercicio = { id: number; ejercicio: number; cerrado: boolean; cerradoEnUtc?: string | null; reabiertoEnUtc?: string | null; observacion?: string | null; asientoCierreId?: number | null; asientoAperturaId?: number | null };
export type EstadoCierres = { ejercicio: number; periodos: CierrePeriodo[]; cierreEjercicio?: CierreEjercicio | null; asientosEjercicio: number };
const query = (ejercicio: number) => `?ejercicio=${ejercicio}`;
export const getCierres = (empresaId: number, ejercicio: number) => request<EstadoCierres>(`/empresas/${empresaId}/cierres${query(ejercicio)}`);
export const cerrarPeriodo = (empresaId: number, ejercicio: number, mes: number, observacion?: string) => request<CierrePeriodo>(`/empresas/${empresaId}/cierres/periodos/${mes}/cerrar${query(ejercicio)}`, { method: 'POST', body: JSON.stringify({ observacion }) });
export const reabrirPeriodo = (empresaId: number, ejercicio: number, mes: number, observacion?: string) => request<CierrePeriodo>(`/empresas/${empresaId}/cierres/periodos/${mes}/reabrir${query(ejercicio)}`, { method: 'POST', body: JSON.stringify({ observacion }) });
export const cerrarEjercicio = (empresaId: number, ejercicio: number, observacion?: string) => request<CierreEjercicio>(`/empresas/${empresaId}/cierres/ejercicio/cerrar${query(ejercicio)}`, { method: 'POST', body: JSON.stringify({ observacion }) });
export const reabrirEjercicio = (empresaId: number, ejercicio: number, observacion?: string) => request<CierreEjercicio>(`/empresas/${empresaId}/cierres/ejercicio/reabrir${query(ejercicio)}`, { method: 'POST', body: JSON.stringify({ observacion }) });
