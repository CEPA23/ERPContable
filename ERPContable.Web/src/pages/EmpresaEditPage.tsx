import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getEmpresaById, updateEmpresa } from '../api/empresas';
import { EmpresaForm, type EmpresaFormValues } from '../components/EmpresaForm';
import type { Empresa } from '../types/empresa';

export function EmpresaEditPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const empresaId = Number(id);
  const [empresa, setEmpresa] = useState<Empresa | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      if (!Number.isFinite(empresaId)) {
        setError('El identificador de la empresa no es válido.');
        setLoading(false);
        return;
      }

      try {
        const data = await getEmpresaById(empresaId);
        setEmpresa(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'No se pudo cargar la empresa.');
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [empresaId]);

  const handleSubmit = async (payload: EmpresaFormValues) => {
    await updateEmpresa(empresaId, payload);
    navigate('/empresas');
  };

  if (loading) {
    return <div className="panel empty-state">Cargando empresa...</div>;
  }

  if (error) {
    return (
      <div className="page-stack">
        <div className="error-box">{error}</div>
        <Link className="secondary-button" to="/empresas">
          Volver al listado
        </Link>
      </div>
    );
  }

  if (!empresa) {
    return (
      <div className="page-stack">
        <div className="panel empty-state">La empresa solicitada no existe.</div>
        <Link className="secondary-button" to="/empresas">
          Volver al listado
        </Link>
      </div>
    );
  }

  return (
    <div className="page-stack">
      <div className="page-header">
        <div>
          <span className="eyebrow">Empresas</span>
          <h1>Editar empresa</h1>
          <p>Actualiza los datos maestros y el estado operativo.</p>
        </div>
      </div>

      <EmpresaForm
        initialValue={empresa}
        submitLabel="Guardar cambios"
        onSubmit={handleSubmit}
      />
    </div>
  );
}
