import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { cambiarContrasena as cambiarContrasenaApi, cerrarSesion, iniciarSesion, restaurarSesion, seleccionarEmpresa as seleccionarEmpresaApi, type EmpresaSesion, type RolEmpresa, type SesionAutenticada, type UsuarioSesion } from '../api/auth';
import { clearAccessToken } from '../api/client';
import type { Empresa } from '../types/empresa';

type Session = {
  ready: boolean;
  user: UsuarioSesion | null;
  empresas: EmpresaSesion[];
  empresa: Empresa | null;
  rol: RolEmpresa | null;
  ejercicio: number | null;
  periodo: number | null;
};
type AppSessionValue = Session & {
  login: (correoElectronico: string, contrasena: string) => Promise<SesionAutenticada>;
  selectEmpresa: (empresa: Empresa) => Promise<SesionAutenticada>;
  selectEjercicio: (ejercicio: number) => void;
  selectPeriodo: (periodo: number) => void;
  logout: () => Promise<void>;
  cambiarContrasena: (contrasenaActual: string, nuevaContrasena: string) => Promise<void>;
  hasRole: (...roles: RolEmpresa[]) => boolean;
};
const SessionContext = createContext<AppSessionValue | null>(null);
const emptySession: Session = { ready: false, user: null, empresas: [], empresa: null, rol: null, ejercicio: null, periodo: null };

function toSession(session: SesionAutenticada, previous?: Session): Session {
  const active = session.empresaActiva;
  return {
    ready: true,
    user: session.usuario,
    empresas: session.empresas,
    empresa: active?.empresa ?? null,
    rol: active?.rol ?? null,
    ejercicio: active ? previous?.ejercicio ?? new Date().getFullYear() : null,
    periodo: active ? previous?.periodo ?? new Date().getMonth() + 1 : null,
  };
}

export function AppSessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session>(emptySession);

  useEffect(() => {
    let active = true;
    restaurarSesion().then((value) => { if (active) setSession((current) => toSession(value, current)); })
      .catch(() => { if (active) setSession({ ...emptySession, ready: true }); });
    return () => { active = false; };
  }, []);

  const value = useMemo<AppSessionValue>(() => ({
    ...session,
    login: async (correoElectronico, contrasena) => {
      const response = await iniciarSesion(correoElectronico, contrasena);
      setSession((current) => toSession(response, current));
      return response;
    },
    selectEmpresa: async (empresa) => {
      const response = await seleccionarEmpresaApi(empresa.id);
      setSession((current) => toSession(response, current));
      return response;
    },
    selectEjercicio: (ejercicio) => setSession((current) => ({ ...current, ejercicio, periodo: null })),
    selectPeriodo: (periodo) => setSession((current) => ({ ...current, periodo })),
    logout: async () => {
      await cerrarSesion();
      setSession({ ...emptySession, ready: true });
    },
    cambiarContrasena: async (contrasenaActual, nuevaContrasena) => {
      await cambiarContrasenaApi(contrasenaActual, nuevaContrasena);
      clearAccessToken();
      setSession({ ...emptySession, ready: true });
    },
    hasRole: (...roles: RolEmpresa[]) => session.rol !== null && roles.includes(session.rol),
  }), [session]);

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useAppSession() {
  const value = useContext(SessionContext);
  if (!value) throw new Error('AppSessionProvider no está configurado.');
  return value;
}
