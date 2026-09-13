import { request } from './client';

export type Cuenta = { id: number; codigo: string; nombre: string; tipo: string; activa: boolean };
export type DetalleAsiento = { cuentaContableId: number; cuenta: string; debe: number; haber: number };
export type Asiento = { id: number; fecha: string; glosa: string; totalDebe: number; totalHaber: number; detalles: DetalleAsiento[] };
export type AsientoCreate = { fecha: string; glosa: string; detalles: { cuentaContableId: number; debe: number; haber: number }[] };

export const getCuentas = (empresaId: number) => request<Cuenta[]>(`/empresas/${empresaId}/contabilidad/cuentas`);
export const createCuenta = (empresaId: number, payload: { codigo: string; nombre: string; tipo: string }) => request<Cuenta>(`/empresas/${empresaId}/contabilidad/cuentas`, { method: 'POST', body: JSON.stringify(payload) });
export const cambiarEstadoCuenta = (empresaId: number, cuentaId: number, activa: boolean) => request<Cuenta>(`/empresas/${empresaId}/contabilidad/cuentas/${cuentaId}/estado`, { method: 'PATCH', body: JSON.stringify({ activa }) });
export const getAsientos = (empresaId: number) => request<Asiento[]>(`/empresas/${empresaId}/contabilidad/asientos`);
export const createAsiento = (empresaId: number, payload: AsientoCreate) => request<Asiento>(`/empresas/${empresaId}/contabilidad/asientos`, { method: 'POST', body: JSON.stringify(payload) });
