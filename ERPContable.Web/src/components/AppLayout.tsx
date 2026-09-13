import { useEffect, useMemo, useState } from 'react';
import { NavLink, Outlet, useLocation } from 'react-router-dom';
import { useAppSession } from '../context/AppSessionContext';
import { systemStatusLabel, useSystemHealth } from '../hooks/useSystemHealth';
import { puedeAcceder } from '../security/permissions';

type IconName = 'dashboard' | 'company' | 'journal' | 'report' | 'close' | 'operations' | 'settings' | 'users' | 'plus' | 'lock';

const navLinkClass = ({ isActive }: { isActive: boolean }) => `nav-link ${isActive ? 'active' : ''}`;

function AppIcon({ name }: { name: IconName }) {
  const paths: Record<IconName, React.ReactNode> = {
    dashboard: <><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /></>,
    company: <><rect x="4" y="3" width="16" height="18" rx="2" /><path d="M8 7h2M14 7h2M8 11h2M14 11h2M8 15h2M14 15h2M10 21v-3h4v3" /></>,
    journal: <><path d="M5 4h14v16H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Z" /><path d="M7 8h9M7 12h9M7 16h5" /></>,
    report: <><path d="M5 3h10l4 4v14H5z" /><path d="M15 3v5h4M8 16v-3M12 16v-5M16 16v-7" /></>,
    close: <><path d="M4 5h16M4 19h16M6 5v14M18 5v14" /><path d="M9 9h6M9 13h6" /></>,
    operations: <><path d="M4 7h16M4 12h16M4 17h16" /><circle cx="8" cy="7" r="2" /><circle cx="16" cy="12" r="2" /><circle cx="10" cy="17" r="2" /></>,
    settings: <><path d="M12 3v3M12 18v3M3 12h3M18 12h3M5.6 5.6l2.1 2.1M16.3 16.3l2.1 2.1M18.4 5.6l-2.1 2.1M7.7 16.3l-2.1 2.1" /><circle cx="12" cy="12" r="4" /></>,
    users: <><circle cx="9" cy="8" r="3" /><path d="M3 20a6 6 0 0 1 12 0M16 5a3 3 0 0 1 0 6M17 14a5 5 0 0 1 4 6" /></>,
    plus: <><path d="M12 5v14M5 12h14" /></>,
    lock: <><rect x="5" y="10" width="14" height="11" rx="2" /><path d="M8 10V7a4 4 0 0 1 8 0v3" /></>,
  };

  void paths;
  const assets: Record<IconName, string> = { dashboard: 'grafico-histograma.svg', company: 'usuarios-alt.svg', journal: 'lista.svg', report: 'grafico-histograma.svg', close: 'actualizar.svg', operations: 'lista.svg', settings: 'editar.svg', users: 'usuarios-alt.svg', plus: 'editar.svg', lock: 'editar.svg' };
  return <img className="nav-icon" src={`/assets/${assets[name]}`} alt="" aria-hidden="true" />;
}

const breadcrumbLabels: Record<string, string> = {
  empresas: 'Empresas',
  contabilidad: 'Contabilidad',
  asientos: 'Asientos contables',
  reportes: 'Reportes contables',
  cierres: 'Cierres contables',
  operaciones: 'Operaciones',
  compras: 'Compras',
  proveedores: 'Proveedores',
  ventas: 'Ventas',
  clientes: 'Clientes',
  caja: 'Caja',
  bancos: 'Bancos',
  activos: 'Activos',
  ajustes: 'Ajustes contables',
  configuracion: 'Configuración',
  usuarios: 'Usuarios y roles',
  'plan-contable': 'Plan contable',
  catalogo: 'Catálogo básico',
  inventario: 'Inventario',
  perfil: 'Perfil',
  contrasena: 'Cambiar contraseña',
  nueva: 'Nueva empresa',
  editar: 'Editar empresa',
};

function Breadcrumbs({ pathname }: { pathname: string }) {
  const items = useMemo(() => {
    if (pathname === '/') return [{ label: 'Dashboard', to: '/' }];
    const segments = pathname.split('/').filter(Boolean);
    const result: { label: string; to?: string }[] = [{ label: 'Inicio', to: '/' }];
    let currentPath = '';
    segments.forEach((segment, index) => {
      currentPath += `/${segment}`;
      const label = breadcrumbLabels[segment] ?? (/^\d+$/.test(segment) || segment === 'id' ? 'Detalle' : segment);
      result.push({ label, to: index === segments.length - 1 ? undefined : currentPath });
    });
    return result;
  }, [pathname]);

  return <nav className="breadcrumbs" aria-label="Ruta de navegación">
    {items.map((item, index) => <span className="breadcrumb-item" key={`${item.label}-${index}`}>
      {index > 0 ? <span className="breadcrumb-separator" aria-hidden="true">/</span> : null}
      {item.to ? <NavLink to={item.to}>{item.label}</NavLink> : <strong aria-current="page">{item.label}</strong>}
    </span>)}
  </nav>;
}

function CollapsibleNavGroup({ id, label, active, children }: { id: string; label: string; active: boolean; children: React.ReactNode }) {
  const storageKey = `erp-nav-group-${id}`;
  const [open, setOpen] = useState(() => localStorage.getItem(storageKey) !== 'closed');

  useEffect(() => {
    if (active) setOpen(true);
  }, [active]);

  const toggle = () => {
    setOpen((current) => {
      const next = !current;
      localStorage.setItem(storageKey, next ? 'open' : 'closed');
      return next;
    });
  };

  return <section className={`nav-group ${open ? 'is-open' : 'is-collapsed'}`}>
    <button className="nav-group-toggle" type="button" onClick={toggle} aria-expanded={open} aria-controls={`nav-group-${id}`}>
      <span className="nav-group-label">{label}</span><span className="nav-group-chevron" aria-hidden="true">⌄</span>
    </button>
    <div id={`nav-group-${id}`} className="nav-group-links">{children}</div>
  </section>;
}

export function AppLayout() {
  const [menuOpen, setMenuOpen] = useState(false);
  const location = useLocation();
  const { user, empresa, ejercicio, periodo, rol, selectEjercicio, selectPeriodo, logout } = useAppSession();
  const meses = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
  const ejercicios = [new Date().getFullYear(), new Date().getFullYear() - 1, new Date().getFullYear() - 2];
  const systemStatus = useSystemHealth();
  const closeMenu = () => setMenuOpen(false);
  const operacionesDisponibles = puedeAcceder(rol, 'compras') || puedeAcceder(rol, 'ventas') || puedeAcceder(rol, 'caja') || puedeAcceder(rol, 'bancos') || puedeAcceder(rol, 'activos') || puedeAcceder(rol, 'ajustes') || puedeAcceder(rol, 'inventario');

  useEffect(() => {
    if (!menuOpen) return;
    const onKeyDown = (event: KeyboardEvent) => { if (event.key === 'Escape') closeMenu(); };
    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  }, [menuOpen]);

  useEffect(() => {
    document.body.style.overflow = menuOpen ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [menuOpen]);

  useEffect(() => {
    const elements = Array.from(document.querySelectorAll<HTMLElement>('.content .page-header, .content .hero, .content .panel, .content .stat-card'));
    elements.forEach((element, index) => { element.classList.add('scroll-reveal'); element.style.setProperty('--reveal-delay', `${Math.min(index * 45, 220)}ms`); });
    if (!('IntersectionObserver' in window)) { elements.forEach((element) => element.classList.add('is-visible')); return; }
    const observer = new IntersectionObserver((entries) => entries.forEach((entry) => { if (entry.isIntersecting) { entry.target.classList.add('is-visible'); observer.unobserve(entry.target); } }), { threshold: .08, rootMargin: '0px 0px -24px' });
    elements.forEach((element) => observer.observe(element));
    return () => observer.disconnect();
  }, [location.pathname]);

  return <div className={`shell ${menuOpen ? 'menu-is-open' : ''}`}>
    <div className="mobile-backdrop" onClick={closeMenu} aria-hidden="true" />
    <aside id="main-navigation" className="sidebar" aria-label="Navegación principal">
      <div className="brand">
        <NavLink className="brand-home-link" to="/" onClick={closeMenu} aria-label="Ir a la página principal"><div className="brand-mark"><img src="/favicon.png?v=2" alt="" /></div><div><strong>ERP Contable</strong><p>Gestión financiera</p></div></NavLink>
        <button className="close-menu" type="button" onClick={closeMenu} aria-label="Cerrar menú">×</button>
      </div>
      <nav className="nav">
        <CollapsibleNavGroup id="inicio" label="Inicio" active={location.pathname === '/' || location.pathname.startsWith('/empresas')}>
          <NavLink to="/" className={navLinkClass} end onClick={closeMenu}><AppIcon name="dashboard" />Dashboard</NavLink>
          <NavLink to="/empresas" className={navLinkClass} end onClick={closeMenu}><AppIcon name="company" />Empresas</NavLink>
        </CollapsibleNavGroup>

        {puedeAcceder(rol, 'contabilidad') || puedeAcceder(rol, 'reportes') || puedeAcceder(rol, 'cierres') ? <CollapsibleNavGroup id="contabilidad" label="Contabilidad" active={location.pathname.startsWith('/contabilidad')}>
          {puedeAcceder(rol, 'contabilidad') ? <NavLink to="/contabilidad/asientos" className={navLinkClass} onClick={closeMenu}><AppIcon name="journal" />Asientos contables</NavLink> : null}
          {puedeAcceder(rol, 'reportes') ? <NavLink to="/contabilidad/reportes" className={navLinkClass} onClick={closeMenu}><AppIcon name="report" />Reportes</NavLink> : null}
          {puedeAcceder(rol, 'cierres') ? <NavLink to="/contabilidad/cierres" className={navLinkClass} onClick={closeMenu}><AppIcon name="close" />Cierres contables</NavLink> : null}
        </CollapsibleNavGroup> : null}

        {operacionesDisponibles ? <CollapsibleNavGroup id="operaciones" label="Operaciones" active={location.pathname.startsWith('/operaciones')}>
          <NavLink to="/operaciones" className={navLinkClass} end onClick={closeMenu}><AppIcon name="operations" />Operaciones diarias</NavLink>
          {puedeAcceder(rol, 'inventario') ? <NavLink to="/operaciones/inventario" className={navLinkClass} onClick={closeMenu}><AppIcon name="operations" />Inventario</NavLink> : null}
        </CollapsibleNavGroup> : null}

        <CollapsibleNavGroup id="configuracion" label="Configuración" active={location.pathname.startsWith('/configuracion') || location.pathname === '/empresas/nueva'}>
          {puedeAcceder(rol, 'configuracion') ? <NavLink to="/configuracion/plan-contable" className={navLinkClass} onClick={closeMenu}><AppIcon name="settings" />Plan contable</NavLink> : null}
          {puedeAcceder(rol, 'catalogo') ? <NavLink to="/configuracion/catalogo" className={navLinkClass} onClick={closeMenu}><AppIcon name="settings" />Catálogo básico</NavLink> : null}
          {puedeAcceder(rol, 'usuarios') ? <NavLink to="/configuracion/usuarios" className={navLinkClass} onClick={closeMenu}><AppIcon name="users" />Usuarios</NavLink> : null}
          <NavLink to="/empresas/nueva" className={navLinkClass} onClick={closeMenu}><AppIcon name="plus" />Nueva empresa</NavLink>
        </CollapsibleNavGroup>
      </nav>
      <div className="sidebar-card"><span className={`status-dot ${systemStatus}`} /><div><span className="eyebrow">Estado del sistema</span><strong>{systemStatusLabel(systemStatus)}</strong><p>{systemStatus === 'ok' ? 'API y base de datos disponibles.' : systemStatus === 'loading' ? 'Comprobando servicios...' : 'Revisa la conexión del servidor.'}</p></div></div>
      <div className="sidebar-footer">ERP Contable · 2026</div>
    </aside>
    <main className="content">
      <header className="topbar">
        <div className="topbar-start">
          <button className="menu-toggle" type="button" onClick={() => setMenuOpen(true)} aria-label="Abrir menú" aria-expanded={menuOpen} aria-controls="main-navigation"><span /><span /><span /></button>
          <Breadcrumbs pathname={location.pathname} />
        </div>
        <div className="topbar-context" aria-label="Contexto contable activo">
          <NavLink className="context-company" to="/seleccion/empresa" title="Cambiar empresa"><span className="context-label">Empresa activa</span><strong>{empresa?.razonSocial}</strong></NavLink>
          <div className="context-divider" aria-hidden="true" />
          <label className="context-select"><span>Ejercicio</span><select value={ejercicio ?? ejercicios[0]} onChange={(event) => selectEjercicio(Number(event.target.value))}>{ejercicios.map((item) => <option key={item} value={item}>{item}</option>)}</select></label>
          <label className="context-select"><span>Período</span><select value={periodo ?? 1} onChange={(event) => selectPeriodo(Number(event.target.value))}>{meses.map((item, index) => <option key={item} value={index + 1}>{item}</option>)}</select></label>
        </div>
        <div className="topbar-actions">
          <NavLink className="user-profile" to="/perfil/contrasena" title="Cambiar contraseña"><span className="avatar">{user?.nombreCompleto.slice(0, 2).toUpperCase()}</span><span className="user-name">{user?.nombreCompleto}</span></NavLink>
          <button className="text-button logout-button" type="button" onClick={() => void logout()}>Salir</button>
        </div>
      </header>
      <div key={location.pathname} className="route-transition"><Outlet /></div>
      <BackToTop />
    </main>
  </div>;
}

function BackToTop() {
  const [visible, setVisible] = useState(false);
  useEffect(() => {
    const content = document.querySelector<HTMLElement>('.content');
    const onScroll = () => setVisible(Math.max(window.scrollY, document.documentElement.scrollTop, document.body.scrollTop, content?.scrollTop ?? 0) > 160);
    window.addEventListener('scroll', onScroll, { passive: true });
    content?.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    return () => { window.removeEventListener('scroll', onScroll); content?.removeEventListener('scroll', onScroll); };
  }, []);
  return visible ? <button className="back-to-top" type="button" onClick={() => { window.scrollTo({ top: 0, behavior: 'smooth' }); document.querySelector<HTMLElement>('.content')?.scrollTo({ top: 0, behavior: 'smooth' }); }} aria-label="Volver arriba" title="Volver arriba">↑</button> : null;
}
