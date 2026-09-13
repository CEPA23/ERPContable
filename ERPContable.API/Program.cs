using System.Security.Claims;
using System.Text;
using ERPContable.API.Filters;
using ERPContable.API.Security;
using ERPContable.API.Services;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using ERPContable.Domain.Entities;
using ERPContable.Infrastructure.Services;
using ERPContable.Persistence.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

namespace ERPContable.API;

public static class ApiHost
{
public static async Task<WebApplication> BuildAsync(
    string[] args,
    string? environmentName = null,
    Action<WebApplicationBuilder>? configureBuilder = null)
{
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(ApiHost).Assembly.FullName,
    ContentRootPath = AppContext.BaseDirectory,
    EnvironmentName = environmentName
});
configureBuilder?.Invoke(builder);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Configura Jwt:Key mediante user-secrets o una variable de entorno.");
if (jwtKey.Length < 32) throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

builder.Services.AddControllers(options => options.Filters.AddService<AuditActionFilter>());
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "PostgreSQL";
builder.Services.AddDbContext<ERPDbContext>(options =>
{
    if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var connectionString = builder.Configuration.GetConnectionString("Sqlite")
            ?? throw new InvalidOperationException("Configura ConnectionStrings:Sqlite para el modo de escritorio.");
        options.UseSqlite(connectionString);
    }
    else if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", ERPDbContext.DefaultSchema));
    }
    else
    {
        throw new InvalidOperationException($"El proveedor de base de datos '{databaseProvider}' no es compatible.");
    }
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.SignIn.RequireConfirmedEmail = builder.Configuration.GetValue("Auth:RequireConfirmedEmail", true);
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ERPDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var tokenStamp = context.Principal?.FindFirstValue(AuthService.SecurityStampClaim);
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = userId is null ? null : await userManager.FindByIdAsync(userId);
                if (user is null || !user.Activo || tokenStamp != await userManager.GetSecurityStampAsync(user)) context.Fail("La sesión ya no es válida.");
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy(ErpPolicies.Compras, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador));
    options.AddPolicy(ErpPolicies.Ventas, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador));
    options.AddPolicy(ErpPolicies.Caja, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador, RolesERP.Cajero));
    options.AddPolicy(ErpPolicies.Bancos, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador, RolesERP.Cajero));
    options.AddPolicy(ErpPolicies.Activos, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador));
    options.AddPolicy(ErpPolicies.Contabilidad, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador));
    options.AddPolicy(ErpPolicies.Ajustes, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador));
    options.AddPolicy(ErpPolicies.Reportes, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador, RolesERP.Gerente, RolesERP.Auditor));
    options.AddPolicy(ErpPolicies.Configuracion, policy => policy.RequireRole(RolesERP.Administrador));
    options.AddPolicy(ErpPolicies.Usuarios, policy => policy.RequireRole(RolesERP.Administrador));
    options.AddPolicy(ErpPolicies.Dashboard, policy => policy.RequireRole(RolesERP.Administrador, RolesERP.Contador, RolesERP.Cajero, RolesERP.Gerente, RolesERP.Auditor));
});

builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IEmpresaAccesoService, EmpresaAccesoService>();
builder.Services.AddScoped<IContabilidadService, ContabilidadService>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IVentasService, VentasService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IBancosService, BancosService>();
builder.Services.AddScoped<IActivosService, ActivosService>();
builder.Services.AddScoped<IAjustesService, AjustesService>();
builder.Services.AddScoped<IReportesContablesService, ReportesContablesService>();
builder.Services.AddScoped<IUsuariosEmpresaService, UsuariosEmpresaService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICierreContableService, CierreContableService>();
builder.Services.AddScoped<IConfiguracionContableService, ConfiguracionContableService>();
builder.Services.AddScoped<CorreccionesService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IConsultaSunatService, ConsultaSunatService>();
builder.Services.AddScoped<ReportesExportService>();
builder.Services.AddHttpClient<IApisPeruService, ApisPeruService>(client =>
{
    client.BaseAddress = new Uri("https://dniruc.apisperu.com/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
    if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        await dbContext.Database.EnsureCreatedAsync();
    else
        await dbContext.Database.MigrateAsync();
    var codigosPlantilla = await dbContext.CuentasContables.Where(x => x.EmpresaId == null).Select(x => x.Codigo).ToListAsync();
    var plantillasFaltantes = PlanContableBase.Cuentas.Where(x => !codigosPlantilla.Contains(x.Codigo, StringComparer.OrdinalIgnoreCase)).Select(x => CuentaContable.Crear(x.Codigo, x.Nombre, x.Tipo)).ToList();
    if (plantillasFaltantes.Count > 0)
    {
        dbContext.CuentasContables.AddRange(plantillasFaltantes);
        await dbContext.SaveChangesAsync();
    }

    var plantillas = await dbContext.CuentasContables.AsNoTracking().Where(x => x.EmpresaId == null).OrderBy(x => x.Codigo).ToListAsync();
    var empresas = await dbContext.Empresas.AsNoTracking().Select(x => x.Id).ToListAsync();
    foreach (var empresaId in empresas)
    {
        var codigosEmpresa = await dbContext.CuentasContables.Where(x => x.EmpresaId == empresaId).Select(x => x.Codigo).ToListAsync();
        var faltantes = plantillas.Where(x => !codigosEmpresa.Contains(x.Codigo, StringComparer.OrdinalIgnoreCase)).Select(x => CuentaContable.CopiarParaEmpresa(empresaId, x)).ToList();
        if (faltantes.Count > 0) dbContext.CuentasContables.AddRange(faltantes);
    }
    if (dbContext.ChangeTracker.HasChanges()) await dbContext.SaveChangesAsync();
}

if ((app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Desktop")) && app.Configuration.GetValue("DemoData:Enabled", true))
{
    await DemoDataSeeder.SeedAsync(app.Services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("Frontend");
if (!app.Environment.IsEnvironment("Desktop")) app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<EmpresaContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
if (File.Exists(Path.Combine(app.Environment.WebRootPath ?? string.Empty, "index.html")))
    app.MapFallbackToFile("index.html").AllowAnonymous();

return app;
}
}
