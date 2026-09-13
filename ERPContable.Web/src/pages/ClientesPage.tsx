import { useEffect, useState } from 'react';
import { createCliente, getClientes, type Cliente } from '../api/ventas';
import { ThirdPartyMaster } from '../components/ThirdPartyMaster';
import { useAppSession } from '../context/AppSessionContext';

export function ClientesPage() {
  const { empresa, ejercicio, periodo } = useAppSession();
  const [items, setItems] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [messageKind, setMessageKind] = useState<'success' | 'error'>('success');
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    if (!empresa) return;
    let active = true;
    setLoading(true); setError(null);
    void getClientes(empresa.id).then(data => { if (active) setItems(data); }).catch(e => { if (active) setError(e instanceof Error ? e.message : 'No se pudieron cargar los clientes.'); }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [empresa?.id, reloadToken]);

  const save = async (payload: { documento: string; razonSocial: string }) => {
    setSaving(true); setMessage(null);
    try { const item = await createCliente(empresa?.id ?? 0, payload); setItems(current => [item, ...current]); setMessage('Cliente registrado.'); setMessageKind('success'); return true; }
    catch (e) { setMessage(e instanceof Error ? e.message : 'No se pudo registrar el cliente.'); setMessageKind('error'); return false; }
    finally { setSaving(false); }
  };

  return <ThirdPartyMaster kind="clientes" companyName={empresa?.razonSocial} ejercicio={ejercicio} periodo={periodo} items={items} loading={loading} error={error} saving={saving} message={message} messageKind={messageKind} onRetry={() => setReloadToken(value => value + 1)} onCreate={save} />;
}
