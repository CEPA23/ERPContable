import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getEmpresas } from '../api/empresas';
import type { Empresa } from '../types/empresa';
import { useAppSession } from '../context/AppSessionContext';

export function EmpresaSelectPage() {
  const navigate = useNavigate();
  const { user, selectEmpresa } = useAppSession();
  const [items, setItems] = useState<Empresa[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);
  useEffect(() => { void getEmpresas().then((empresas) => { if (empresas.length === 0) navigate('/onboarding/empresa', { replace: true }); else setItems(empresas); }).catch((err) => setError(err instanceof Error ? err.message : 'No se pudieron cargar las empresas.')); }, [navigate]);
  const select = async (empresa: Empresa) => { setBusyId(empresa.id); setError(null); try { await selectEmpresa(empresa); navigate('/', { replace: true }); } catch (err) { setError(err instanceof Error ? err.message : 'No se pudo seleccionar la empresa.'); } finally { setBusyId(null); } };
  return <div className="flow-shell"><div className="flow-card panel"><span className="eyebrow">{user?.nombreCompleto} · Contexto de trabajo</span><h1>Seleccionar empresa</h1><p>Elige la empresa sobre la que trabajarás. El ejercicio y el mes se cambian desde la barra superior.</p>{error ? <div className="error-box">{error}</div> : null}<div className="selection-grid">{items.filter((item) => item.activa).map((item) => <button className="selection-card" key={item.id} disabled={busyId !== null} onClick={() => void select(item)}><strong>{item.razonSocial}</strong><span>{item.documentoIdentidad}</span><span>{busyId === item.id ? 'Seleccionando...' : 'Ingresar'}</span></button>)}</div>{items.length === 0 && !error ? <div className="empty-state">No hay empresas activas registradas.</div> : null}</div></div>;
}
