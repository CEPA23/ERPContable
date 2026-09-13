# ERP Contable

Aplicación local con frontend React/Vite, API ASP.NET Core y PostgreSQL.

## Requisitos

- .NET SDK 10
- Node.js 22 o superior
- Docker Desktop (recomendado para PostgreSQL)

## Puesta en marcha

Desde la raíz del proyecto:

```powershell
Copy-Item .env.docker.example .env
# Edita .env y reemplaza POSTGRES_PASSWORD antes de continuar.
docker compose up -d postgres
```

Configura la clave JWT y la contraseña de la base de datos como secretos locales de .NET:

```powershell
cd .\ERPContable.API
dotnet user-secrets init
$jwtKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "Jwt:Key" $jwtKey
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ERPContableDB;Username=erp_admin;Password=LA_CLAVE_DE_TU_ARCHIVO_ENV"
dotnet restore
dotnet run --launch-profile http
```

En otra terminal:

```powershell
cd .\ERPContable.Web
if (!(Test-Path .env)) { Copy-Item .env.example .env }
npm install
npm run dev
```

Abre <http://localhost:5173>. La API queda disponible en <http://localhost:5255> y Swagger en <http://localhost:5255/swagger>.

## Acceso inicial

No existe una contraseña predeterminada. En la pantalla de inicio selecciona **Crear cuenta**. El primer usuario registrado obtiene permisos de administrador. En desarrollo no se exige confirmación por correo.

La contraseña debe tener al menos 10 caracteres, una mayúscula, una minúscula, un número y un símbolo.

Al reiniciar la API en Development después de registrar el primer usuario, se cargan automáticamente datos demo idempotentes para **Comercial Andina S.A.C.**: plan contable, clientes, proveedor, compras, ventas, caja, banco, activo fijo y asientos. Para desactivarlos, cambia `DemoData:Enabled` a `false` en `appsettings.Development.json`.

## Detener PostgreSQL

```powershell
docker compose stop postgres
```

Para eliminar también los datos locales (acción irreversible):

```powershell
docker compose down -v
```

## Aplicación de escritorio para Windows

La edición de escritorio integra React, la API ASP.NET Core, WebView2 y una base
SQLite local. Para generar el paquete autocontenido de Windows x64:

```powershell
.\build-desktop.ps1
```

El resultado se guarda en `artifacts\ERPContable-Desktop-win-x64.zip`. El cliente
solo debe extraer todo el ZIP y abrir `ERPContable.exe`; no necesita instalar .NET,
Node.js, PostgreSQL ni Docker. Los datos se guardan en
`%LOCALAPPDATA%\ERPContable\Data\erpcontable.db`.
