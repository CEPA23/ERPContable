import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="panel empty-state">
      <h1>404</h1>
      <p>La ruta solicitada no existe.</p>
      <Link className="primary-button" to="/">
        Ir al inicio
      </Link>
    </div>
  );
}
