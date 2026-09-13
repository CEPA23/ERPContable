import { request } from './client';
export type CuentaBancaria = { id: number; banco: string; numeroCuenta: string; moneda: string; cuentaContable: string; activa: boolean };
export type MovimientoBanco = { id: number; fecha: string; banco: string; numeroCuenta: string; tipo: string; descripcion: string; monto: number; cuentaContrapartida: string; asientoContableId?: number };
export const getCuentasBancarias = (empresaId: number) => request<CuentaBancaria[]>(`/empresas/${empresaId}/bancos/cuentas`);
export const createCuentaBancaria = (empresaId: number, payload: { banco: string; numeroCuenta: string; moneda: string; cuentaContableId: number }) => request<CuentaBancaria>(`/empresas/${empresaId}/bancos/cuentas`, { method: 'POST', body: JSON.stringify(payload) });
export const getMovimientosBanco = (empresaId: number) => request<MovimientoBanco[]>(`/empresas/${empresaId}/bancos`);
export const createMovimientoBanco = (empresaId: number, payload: { cuentaBancariaId: number; fecha: string; tipo: string; descripcion: string; monto: number; cuentaContrapartidaId: number }) => request<MovimientoBanco>(`/empresas/${empresaId}/bancos`, { method: 'POST', body: JSON.stringify(payload) });
