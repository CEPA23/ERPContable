import { request } from './client';

export type ConfiguracionContable = {
  empresaId: number;
  cuentaResultadoAcumuladoId?: number | null;
  cuentaResultadoAcumulado?: string | null;
  cuentaInventarioId?: number | null;
  cuentaInventario?: string | null;
  cuentaCostoVentasId?: number | null;
  cuentaCostoVentas?: string | null;
};

export const getConfiguracionContable = (empresaId: number) => request<ConfiguracionContable>(`/empresas/${empresaId}/configuracion-contable`);
export const saveConfiguracionContable = (empresaId: number, payload: { cuentaResultadoAcumuladoId: number; cuentaInventarioId: number; cuentaCostoVentasId: number }) => request<ConfiguracionContable>(`/empresas/${empresaId}/configuracion-contable`, {
  method: 'PUT', body: JSON.stringify(payload),
});
