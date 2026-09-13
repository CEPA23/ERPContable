import { request } from './client';
export type Activo = { id: number; codigo: string; descripcion: string; fechaAdquisicion: string; valorAdquisicion: number; vidaUtilMeses: number; depreciacionMensual: number; depreciacionAcumulada: number; valorNeto: number; activo: boolean };
export const getActivos = (empresaId: number) => request<Activo[]>(`/empresas/${empresaId}/activos`);
export const createActivo = (empresaId: number, payload: { codigo: string; descripcion: string; fechaAdquisicion: string; valorAdquisicion: number; vidaUtilMeses: number; cuentaActivoId: number; cuentaDepreciacionId: number; cuentaGastoId: number }) => request<Activo>(`/empresas/${empresaId}/activos`, { method: 'POST', body: JSON.stringify(payload) });
export const depreciarActivo = (empresaId: number, activoId: number, payload: { fecha: string; importe?: number }) => request<Activo>(`/empresas/${empresaId}/activos/${activoId}/depreciar`, { method: 'POST', body: JSON.stringify(payload) });
