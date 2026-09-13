import { apiFetch, request } from './client';

export type ReporteKey = 'libro-diario' | 'libro-mayor' | 'balance-comprobacion' | 'estado-resultados' | 'balance-general' | 'flujo-caja';
export const reportes: { key: ReporteKey; label: string }[] = [
  { key: 'libro-diario', label: 'Libro Diario' }, { key: 'libro-mayor', label: 'Libro Mayor' },
  { key: 'balance-comprobacion', label: 'Balance de Comprobación' }, { key: 'estado-resultados', label: 'Estado de Resultados' },
  { key: 'balance-general', label: 'Balance General' }, { key: 'flujo-caja', label: 'Flujo de Caja' },
];

const query = (desde: string, hasta: string) => `desde=${encodeURIComponent(desde)}&hasta=${encodeURIComponent(hasta)}`;
export const getReporte = (empresaId: number, reporte: ReporteKey, desde: string, hasta: string) => request<unknown>(`/empresas/${empresaId}/reportes/${reporte}?${query(desde, hasta)}`);
export const downloadReporte = async (empresaId: number, reporte: ReporteKey, formato: 'excel' | 'pdf', desde: string, hasta: string) => {
  const response = await apiFetch(`/empresas/${empresaId}/reportes/${reporte}/${formato}?${query(desde, hasta)}`);
  if (!response.ok) throw new Error((await response.text()) || `Error HTTP ${response.status}`);
  const url = URL.createObjectURL(await response.blob());
  const anchor = document.createElement('a'); anchor.href = url; anchor.download = `${reporte}.${formato === 'excel' ? 'xlsx' : 'pdf'}`; anchor.click(); URL.revokeObjectURL(url);
};
