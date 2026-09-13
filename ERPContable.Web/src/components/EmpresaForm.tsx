import { useState, type FormEvent } from 'react';
import type { Empresa, EmpresaCreatePayload } from '../types/empresa';
import { consultarDocumento } from '../api/empresas';

export type EmpresaFormValues = EmpresaCreatePayload & {
  activa: boolean;
};

type EmpresaFormProps = {
  initialValue?: Empresa | null;
  onSubmit: (payload: EmpresaFormValues) => Promise<void>;
  submitLabel: string;
  busy?: boolean;
  soloRuc?: boolean;
};

function toFormValues(initialValue?: Empresa | null) {
  return {
    razonSocial: initialValue?.razonSocial ?? '',
    documentoIdentidad: initialValue?.documentoIdentidad ?? '',
    nombreComercial: initialValue?.nombreComercial ?? '',
    direccion: initialValue?.direccion ?? '',
    telefono: initialValue?.telefono ?? '',
    email: initialValue?.email ?? '',
    estadoSunat: initialValue?.estadoSunat ?? '',
    condicionSunat: initialValue?.condicionSunat ?? '',
    activa: initialValue?.activa ?? true,
  };
}

export function EmpresaForm({ initialValue, onSubmit, submitLabel, busy, soloRuc = false }: EmpresaFormProps) {
  const [form, setForm] = useState(() => toFormValues(initialValue));
  const [error, setError] = useState<string | null>(null);
  const [lookingUp, setLookingUp] = useState(false);
  const [lookupMessage, setLookupMessage] = useState<string | null>(null);

  const handleChange = (field: keyof typeof form, value: string | boolean) => {
    setForm((current) => ({ ...current, [field]: value }));
  };

  const handleLookup = async () => {
    const documento = form.documentoIdentidad.replace(/\D/g, '');
    if ((soloRuc && documento.length !== 11) || (!soloRuc && documento.length !== 8 && documento.length !== 11)) {
      setLookupMessage('Ingresa un DNI de 8 dígitos o un RUC de 11 dígitos.');
      return;
    }

    setLookingUp(true);
    setLookupMessage(null);
    try {
      const data = await consultarDocumento(documento);
      if (!data) {
        setLookupMessage('No se encontró información para ese documento.');
        return;
      }
      setForm((current) => ({
        ...current,
        documentoIdentidad: documento,
        razonSocial: data.razonSocial ?? data.nombreCompleto ?? current.razonSocial,
        nombreComercial: data.nombreComercial ?? current.nombreComercial,
        direccion: data.direccion ?? current.direccion,
        estadoSunat: data.estado ?? current.estadoSunat,
        condicionSunat: data.condicion ?? current.condicionSunat,
      }));
      setLookupMessage(data.estado || data.condicion ? `Datos completados. Estado: ${data.estado ?? 'sin dato'} · Condición: ${data.condicion ?? 'sin dato'}.` : 'Datos encontrados y completados.');
    } catch (err) {
      setLookupMessage(err instanceof Error ? err.message : 'No se pudo consultar el documento.');
    } finally {
      setLookingUp(false);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    try {
      await onSubmit(form);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'No se pudo guardar la empresa.');
    }
  };

  return (
    <form className="panel form-panel admin-company-form" onSubmit={handleSubmit}>
      <div className="panel-head">
        <div>
          <span className="eyebrow">Datos de empresa</span>
          <h2>{submitLabel}</h2>
        </div>
      </div>

      <div className="admin-form-section">
        <div className="admin-form-section-title"><strong>Identificación</strong><span>Datos principales de la entidad contable</span></div>
        <div className="form-grid">
        <label>
          <span>Razón social</span>
          <input
            value={form.razonSocial}
            onChange={(event) => handleChange('razonSocial', event.target.value)}
            placeholder="ERP Contable SAC"
            required
          />
        </label>
        <label>
          <span>Documento</span>
          <div className="document-lookup">
            <input
              value={form.documentoIdentidad}
              onChange={(event) => handleChange('documentoIdentidad', event.target.value.replace(/\D/g, ''))}
              placeholder={soloRuc ? "20123456789" : "DNI o RUC"}
              inputMode="numeric"
              maxLength={11}
              required
            />
            <button type="button" className="lookup-button" onClick={() => void handleLookup()} disabled={lookingUp}>
              {lookingUp ? 'Consultando...' : 'Consultar'}
            </button>
          </div>
          {lookupMessage ? <small className="lookup-message">{lookupMessage}</small> : null}
        </label>
        </div>
      </div>
      <div className="admin-form-section">
        <div className="admin-form-section-title"><strong>Contacto y denominación</strong><span>Información operativa disponible</span></div>
        <div className="form-grid">
        <label>
          <span>Nombre comercial</span>
          <input
            value={form.nombreComercial}
            onChange={(event) => handleChange('nombreComercial', event.target.value)}
            placeholder="ERP Contable"
          />
        </label>
        <label>
          <span>Teléfono</span>
          <input
            value={form.telefono}
            onChange={(event) => handleChange('telefono', event.target.value)}
            placeholder="999 888 777"
          />
        </label>
        <label className="span-2">
          <span>Dirección</span>
          <input
            value={form.direccion}
            onChange={(event) => handleChange('direccion', event.target.value)}
            placeholder="Av. Principal 123"
          />
        </label>
        </div>
      </div>
      <div className="admin-form-section admin-form-section-last">
        <div className="admin-form-section-title"><strong>Información fiscal y estado</strong><span>Datos consultados y control operativo</span></div>
        <div className="form-grid">
        {form.estadoSunat || form.condicionSunat ? <div className="span-2 success-box">SUNAT: {form.estadoSunat || 'Estado no informado'} · {form.condicionSunat || 'Condición no informada'}</div> : null}
        <label className="span-2">
          <span>Email</span>
          <input
            value={form.email}
            onChange={(event) => handleChange('email', event.target.value)}
            placeholder="contacto@empresa.com"
            type="email"
          />
        </label>
        {initialValue ? (
          <label className="checkbox">
            <input
              type="checkbox"
              checked={form.activa}
              onChange={(event) => handleChange('activa', event.target.checked)}
            />
            <span>Empresa activa</span>
          </label>
        ) : null}
        </div>
      </div>

      {error ? <div className="error-box">{error}</div> : null}

      <div className="form-actions">
        <button className="primary-button" type="submit" disabled={busy}>
          {busy ? 'Guardando...' : submitLabel}
        </button>
      </div>
    </form>
  );
}
