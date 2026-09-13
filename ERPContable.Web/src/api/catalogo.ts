import { request } from './client';

export type Categoria = { id: number; nombre: string; descripcion?: string | null; activa: boolean };
export type Unidad = { id: number; codigo: string; nombre: string; abreviatura: string; activa: boolean };
export type Impuesto = { id: number; codigo: string; nombre: string; tasa: number; activa: boolean };
export type Producto = { id: number; tipo: 'PRODUCTO' | 'SERVICIO'; codigo: string; nombre: string; descripcion?: string | null; categoriaId?: number | null; categoria?: string | null; unidadMedidaId: number; unidadMedida: string; impuestoId: number; impuesto: string; precioVenta: number; costoReferencial: number; activo: boolean };
export type Catalogo = { productos: Producto[]; categorias: Categoria[]; unidades: Unidad[]; impuestos: Impuesto[]; centrosCosto: CentroCosto[] };
export type CentroCosto = { id: number; codigo: string; nombre: string; descripcion?: string | null; activo: boolean };

const base = (empresaId: number) => `/empresas/${empresaId}/catalogo`;
export const getCatalogo = (empresaId: number) => request<Catalogo>(base(empresaId));
export const createCategoria = (empresaId: number, payload: { nombre: string; descripcion?: string }) => request<Categoria>(`${base(empresaId)}/categorias`, { method: 'POST', body: JSON.stringify(payload) });
export const createUnidad = (empresaId: number, payload: { codigo: string; nombre: string; abreviatura: string }) => request<Unidad>(`${base(empresaId)}/unidades`, { method: 'POST', body: JSON.stringify(payload) });
export const createImpuesto = (empresaId: number, payload: { codigo: string; nombre: string; tasa: number }) => request<Impuesto>(`${base(empresaId)}/impuestos`, { method: 'POST', body: JSON.stringify(payload) });
export const createProducto = (empresaId: number, payload: { tipo: string; codigo: string; nombre: string; descripcion?: string; categoriaId?: number | null; unidadMedidaId: number; impuestoId: number; precioVenta: number; costoReferencial: number }) => request<Producto>(`${base(empresaId)}/productos`, { method: 'POST', body: JSON.stringify(payload) });
export const createCentroCosto = (empresaId: number, payload: { codigo: string; nombre: string; descripcion?: string }) => request<CentroCosto>(`${base(empresaId)}/centros-costo`, { method: 'POST', body: JSON.stringify(payload) });
