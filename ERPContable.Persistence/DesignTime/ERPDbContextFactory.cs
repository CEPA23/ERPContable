using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ERPContable.Persistence.Data;

namespace ERPContable.Persistence.DesignTime;

public sealed class ERPDbContextFactory : IDesignTimeDbContextFactory<ERPDbContext>
{
    public ERPDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ERPDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ERP_CONTABLE_DESIGN_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=ERPContableDB;Username=erp_admin";
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", ERPDbContext.DefaultSchema));

        return new ERPDbContext(optionsBuilder.Options);
    }
}
