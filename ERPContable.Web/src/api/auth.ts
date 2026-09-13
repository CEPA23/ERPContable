import { clearAccessToken, csrfToken, publicRequest, request, setAccessToken } from './client';
import type { Empresa } from '../types/empresa';

export type UsuarioSesion = { id: string; nombreCompleto: string; correoElectronico: string; correoConfirmado: boolean };
export type EmpresaSesion = { empresa: Empresa; rol: RolEmpresa };
export type RolEmpresa = 'Administrador' | 'Contador' | 'Cajero' | 'Gerente' | 'Auditor';
export type SesionAutenticada = { accessToken: string; expiraEnUtc: string; usuario: UsuarioSesion; empresas: EmpresaSesion[]; empresaActiva: EmpresaSesion | null };

function saveToken(session: SesionAutenticada) {
  setAccessToken(session.accessToken);
  return session;
}

export const iniciarSesion = async (correoElectronico: string, contrasena: string) => saveToken(await publicRequest<SesionAutenticada>('/auth/inicio-sesion', { method: 'POST', body: JSON.stringify({ correoElectronico, contrasena }) }));
export const registrar = (nombreCompleto: string, correoElectronico: string, contrasena: string) => publicRequest<{ mensaje: string; requiereConfirmacionCorreo: boolean }>('/auth/registro', { method: 'POST', body: JSON.stringify({ nombreCompleto, correoElectronico, contrasena }) });
export const seleccionarEmpresa = async (empresaId: number) => saveToken(await request<SesionAutenticada>('/auth/seleccionar-empresa', { method: 'POST', body: JSON.stringify({ empresaId }) }));

export async function restaurarSesion() {
  const session = await publicRequest<SesionAutenticada>('/auth/renovar', { method: 'POST', headers: { 'X-CSRF-TOKEN': csrfToken() } });
  return saveToken(session);
}

export async function cerrarSesion() {
  try { await publicRequest<void>('/auth/cerrar-sesion', { method: 'POST', headers: { 'X-CSRF-TOKEN': csrfToken() } }); }
  finally { clearAccessToken(); }
}

export const cambiarContrasena = (contrasenaActual: string, nuevaContrasena: string) => request<void>('/auth/cambiar-contrasena', { method: 'POST', body: JSON.stringify({ contrasenaActual, nuevaContrasena }) });
export const solicitarRecuperacion = (correoElectronico: string) => publicRequest<void>('/auth/recuperar-contrasena', { method: 'POST', body: JSON.stringify({ correoElectronico }) });
export const restablecerContrasena = (correoElectronico: string, token: string, nuevaContrasena: string) => publicRequest<void>('/auth/restablecer-contrasena', { method: 'POST', body: JSON.stringify({ correoElectronico, token, nuevaContrasena }) });
export const confirmarCorreo = (userId: string, token: string) => publicRequest<void>(`/auth/confirmar-correo?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`, { method: 'POST' });
