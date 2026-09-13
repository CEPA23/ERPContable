export type Empresa = {
  id: number;
  razonSocial: string;
  documentoIdentidad: string;
  nombreComercial?: string | null;
  direccion?: string | null;
  telefono?: string | null;
  email?: string | null;
  activa: boolean;
  creadaEnUtc: string;
  actualizadaEnUtc?: string | null;
  estadoSunat?: string | null;
  condicionSunat?: string | null;
};

export type EmpresaCreatePayload = {
  razonSocial: string;
  documentoIdentidad: string;
  nombreComercial: string;
  direccion: string;
  telefono: string;
  email: string;
  estadoSunat?: string | null;
  condicionSunat?: string | null;
};

export type EmpresaUpdatePayload = EmpresaCreatePayload & {
  activa: boolean;
};
