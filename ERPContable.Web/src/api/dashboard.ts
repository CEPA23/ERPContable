import { request } from './client';

export type DashboardSerie = { etiqueta: string; ventas: number; compras: number; ingresosCaja: number; egresosCaja: number };
export type DashboardResumen = { ventasMes: number; comprasMes: number; ingresosCajaMes: number; egresosCajaMes: number; tendencia: DashboardSerie[] };
export const getDashboardResumen = (empresaId: number, ejercicio: number, mes: number) => request<DashboardResumen>(`/empresas/${empresaId}/dashboard?ejercicio=${ejercicio}&mes=${mes}`);
