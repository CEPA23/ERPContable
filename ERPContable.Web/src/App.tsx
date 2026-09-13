import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { AppLayout } from './components/AppLayout';
import { DashboardPage } from './pages/DashboardPage';
import { EmpresaCreatePage } from './pages/EmpresaCreatePage';
import { EmpresaEditPage } from './pages/EmpresaEditPage';
import { EmpresasPage } from './pages/EmpresasPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { AsientosPage } from './pages/AsientosPage';
import { ReportesPage } from './pages/ReportesPage';
import { LoginPage } from './pages/LoginPage';
import { EmpresaSelectPage } from './pages/EmpresaSelectPage';
import { EjercicioSelectPage } from './pages/EjercicioSelectPage';
import { PeriodoSelectPage } from './pages/PeriodoSelectPage';
import { OperacionesPage } from './pages/OperacionesPage';
import { OnboardingEmpresaPage } from './pages/OnboardingEmpresaPage';
import { ComprasPage } from './pages/ComprasPage';
import { ProveedoresPage } from './pages/ProveedoresPage';
import { VentasPage } from './pages/VentasPage';
import { ClientesPage } from './pages/ClientesPage';
import { CajaPage } from './pages/CajaPage';
import { BancosPage } from './pages/BancosPage';
import { ActivosPage } from './pages/ActivosPage';
import { AjustesPage } from './pages/AjustesPage';
import { RegistroPage } from './pages/RegistroPage';
import { RecuperarContrasenaPage } from './pages/RecuperarContrasenaPage';
import { RestablecerContrasenaPage } from './pages/RestablecerContrasenaPage';
import { ConfirmarCorreoPage } from './pages/ConfirmarCorreoPage';
import { UsuariosPage } from './pages/UsuariosPage';
import { CambiarContrasenaPage } from './pages/CambiarContrasenaPage';
import { CierresPage } from './pages/CierresPage';
import { PlanContablePage } from './pages/PlanContablePage';
import { CatalogoPage } from './pages/CatalogoPage';
import { InventarioPage } from './pages/InventarioPage';
import { useAppSession } from './context/AppSessionContext';
import { puedeAcceder, type Modulo } from './security/permissions';
import { LoadingScreen } from './components/LoadingScreen';

function FlowGate({ children }: { children: React.ReactNode }) {
  const location = useLocation(); const session = useAppSession();
  if (!session.ready) return <LoadingScreen message="Verificando sesión segura..." />;
  if (!session.user) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (!session.empresa) return <Navigate to="/seleccion/empresa" replace />;
  return <>{children}</>;
}

function ProtectedLayout() { return <FlowGate><AppLayout /></FlowGate>; }
 function SessionGate({ children }: { children: React.ReactNode }) { const session = useAppSession(); if (!session.ready) return <LoadingScreen message="Verificando sesión segura..." />; return session.user ? <>{children}</> : <Navigate to="/login" replace />; }
function ModuleGate({ module, children }: { module: Modulo; children: React.ReactNode }) { const { rol } = useAppSession(); return puedeAcceder(rol, module) ? <>{children}</> : <Navigate to="/" replace />; }

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/registro" element={<RegistroPage />} />
      <Route path="/recuperar-contrasena" element={<RecuperarContrasenaPage />} />
      <Route path="/restablecer-contrasena" element={<RestablecerContrasenaPage />} />
      <Route path="/confirmar-correo" element={<ConfirmarCorreoPage />} />
      <Route path="/seleccion/empresa" element={<SessionGate><EmpresaSelectPage /></SessionGate>} />
      <Route path="/onboarding/empresa" element={<SessionGate><OnboardingEmpresaPage /></SessionGate>} />
      <Route path="/seleccion/ejercicio" element={<SessionGate><EjercicioSelectPage /></SessionGate>} />
      <Route path="/seleccion/periodo" element={<SessionGate><PeriodoSelectPage /></SessionGate>} />
      <Route element={<ProtectedLayout />}>
        <Route index element={<DashboardPage />} />
        <Route path="empresas" element={<EmpresasPage />} />
        <Route path="empresas/nueva" element={<EmpresaCreatePage />} />
        <Route path="empresas/:id/editar" element={<EmpresaEditPage />} />
        <Route path="contabilidad/asientos" element={<ModuleGate module="contabilidad"><AsientosPage /></ModuleGate>} />
        <Route path="contabilidad/reportes" element={<ModuleGate module="reportes"><ReportesPage /></ModuleGate>} />
        <Route path="contabilidad/cierres" element={<ModuleGate module="cierres"><CierresPage /></ModuleGate>} />
        <Route path="operaciones" element={<OperacionesPage />} />
        <Route path="operaciones/compras" element={<ModuleGate module="compras"><ComprasPage /></ModuleGate>} />
        <Route path="operaciones/proveedores" element={<ModuleGate module="compras"><ProveedoresPage /></ModuleGate>} />
        <Route path="operaciones/ventas" element={<ModuleGate module="ventas"><VentasPage /></ModuleGate>} />
        <Route path="operaciones/clientes" element={<ModuleGate module="ventas"><ClientesPage /></ModuleGate>} />
        <Route path="operaciones/caja" element={<ModuleGate module="caja"><CajaPage /></ModuleGate>} />
        <Route path="operaciones/bancos" element={<ModuleGate module="bancos"><BancosPage /></ModuleGate>} />
        <Route path="operaciones/activos" element={<ModuleGate module="activos"><ActivosPage /></ModuleGate>} />
        <Route path="operaciones/ajustes" element={<ModuleGate module="ajustes"><AjustesPage /></ModuleGate>} />
        <Route path="operaciones/inventario" element={<ModuleGate module="inventario"><InventarioPage /></ModuleGate>} />
        <Route path="configuracion/usuarios" element={<ModuleGate module="usuarios"><UsuariosPage /></ModuleGate>} />
        <Route path="configuracion/plan-contable" element={<ModuleGate module="configuracion"><PlanContablePage /></ModuleGate>} />
        <Route path="configuracion/catalogo" element={<ModuleGate module="catalogo"><CatalogoPage /></ModuleGate>} />
        <Route path="perfil/contrasena" element={<CambiarContrasenaPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
