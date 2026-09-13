import { useNavigate } from 'react-router-dom';
import { createEmpresa } from '../api/empresas';
import { EmpresaForm, type EmpresaFormValues } from '../components/EmpresaForm';

export function EmpresaCreatePage() {
  const navigate = useNavigate();

  const handleSubmit = async (payload: EmpresaFormValues) => {
    const created = await createEmpresa({
      razonSocial: payload.razonSocial,
      documentoIdentidad: payload.documentoIdentidad,
      nombreComercial: payload.nombreComercial,
      direccion: payload.direccion,
      telefono: payload.telefono,
      email: payload.email,
      estadoSunat: payload.estadoSunat,
      condicionSunat: payload.condicionSunat,
    });
    navigate(`/empresas/${created.id}/editar`);
  };

  return (
    <div className="page-stack">
      <div className="page-header">
        <div>
          <span className="eyebrow">Empresas</span>
          <h1>Nueva empresa</h1>
          <p>Registra los datos principales para dejarla lista en el sistema.</p>
        </div>
      </div>

      <EmpresaForm submitLabel="Crear empresa" onSubmit={handleSubmit} />
    </div>
  );
}
