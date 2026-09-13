import { request } from './client';
import type { Empresa, EmpresaCreatePayload, EmpresaUpdatePayload } from '../types/empresa';

export const getEmpresas = () => request<Empresa[]>('/empresas');
export const getEmpresaById = (id: number) => request<Empresa>(`/empresas/${id}`);
export const createEmpresa = (payload: EmpresaCreatePayload) => request<Empresa>('/empresas', { method: 'POST', body: JSON.stringify(payload) });
export const updateEmpresa = (id: number, payload: EmpresaUpdatePayload) => request<void>(`/empresas/${id}`, { method: 'PUT', body: JSON.stringify(payload) });
export const deleteEmpresa = (id: number) => request<void>(`/empresas/${id}`, { method: 'DELETE' });

export type ConsultaDocumento = { ruc?: string; dni?: string; razonSocial?: string; nombreCompleto?: string; nombreComercial?: string; direccion?: string; estado?: string; condicion?: string };
export const consultarDocumento = (documento: string) => request<ConsultaDocumento>(`/consultas/${documento.length === 11 ? 'ruc' : 'dni'}/${documento}`);
