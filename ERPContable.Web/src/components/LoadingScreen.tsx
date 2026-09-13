export function LoadingScreen({ message = 'Cargando ERP Contable...' }: { message?: string }) {
  return <div className="loading-screen" role="status" aria-live="polite">
    <div className="loading-brand-mark">EC</div>
    <div className="loading-copy"><strong>ERP Contable</strong><span>{message}</span></div>
    <div className="loading-bar" aria-hidden="true"><i /></div>
  </div>;
}
