import { useNavigate } from 'react-router-dom';
import { createEmpresa } from '../api/empresas';
import { EmpresaForm, type EmpresaFormValues } from '../components/EmpresaForm';
import { useAppSession } from '../context/AppSessionContext';

export function OnboardingEmpresaPage() {
  const navigate = useNavigate();
  const { selectEmpresa } = useAppSession();
  const handleSubmit = async (payload: EmpresaFormValues) => {
    const empresa = await createEmpresa({ razonSocial: payload.razonSocial, documentoIdentidad: payload.documentoIdentidad, nombreComercial: payload.nombreComercial, direccion: payload.direccion, telefono: payload.telefono, email: payload.email, estadoSunat: payload.estadoSunat, condicionSunat: payload.condicionSunat });
    await selectEmpresa(empresa);
    localStorage.setItem(`erpcontable:empresa:${empresa.id}:plan`, 'PCGE');
    navigate('/seleccion/ejercicio', { replace: true });
  };
  return <div className="flow-shell"><div className="flow-card setup-wizard"><div className="setup-progress" aria-label="Progreso de configuración"><span className="is-current"><b>1</b> Empresa</span><i /><span><b>2</b> Ejercicio</span><i /><span><b>3</b> Período</span></div><span className="eyebrow">Configuración inicial</span><h1>Prepara tu empresa</h1><p>Completa estos datos para dejar lista tu primera empresa antes de registrar operaciones.</p><section className="setup-choice"><div><strong>Moneda base</strong><small>La contabilidad actual está preparada para operar en soles.</small></div><div className="setup-value" aria-label="Moneda base: Soles">Soles (PEN)</div></section><div className="setup-note"><strong>Plan contable incluido</strong><span>Se asignará automáticamente el PCGE operativo con cuentas de caja, bancos, IGV, compras, ventas y resultados.</span></div><EmpresaForm submitLabel="Guardar empresa y continuar" onSubmit={handleSubmit} soloRuc /></div></div>;
}
