import { request } from './client';
import type { RolEmpresa } from './auth';

export type UsuarioEmpresa = { usuarioId: string; nombreCompleto: string; correoElectronico: string; rol: RolEmpresa; activo: boolean };
export const getUsuariosEmpresa = (empresaId: number) => request<UsuarioEmpresa[]>(`/empresas/${empresaId}/usuarios`);
export const agregarUsuarioEmpresa = (empresaId: number, correoElectronico: string, rol: RolEmpresa) => request<void>(`/empresas/${empresaId}/usuarios`, { method: 'POST', body: JSON.stringify({ correoElectronico, rol }) });
export const actualizarUsuarioEmpresa = (empresaId: number, usuarioId: string, rol: RolEmpresa, activo: boolean) => request<void>(`/empresas/${empresaId}/usuarios/${encodeURIComponent(usuarioId)}`, { method: 'PUT', body: JSON.stringify({ rol, activo }) });
