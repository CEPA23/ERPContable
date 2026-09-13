import { request } from './client';
export type Ajuste = { id: number; fecha: string; glosa: string; asientoContableId: number; totalDebe: number; totalHaber: number; detalles: { cuenta: string; debe: number; haber: number }[] };
export const getAjustes = (empresaId: number) => request<Ajuste[]>(`/empresas/${empresaId}/ajustes`);
export const createAjuste = (empresaId: number, payload: { fecha: string; glosa: string; detalles: { cuentaContableId: number; debe: number; haber: number }[] }) => request<Ajuste>(`/empresas/${empresaId}/ajustes`, { method: 'POST', body: JSON.stringify(payload) });
