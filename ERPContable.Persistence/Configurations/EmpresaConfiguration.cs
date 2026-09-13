using ERPContable.Persistence.Data;
using ERPContable.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPContable.Persistence.Configurations;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas", ERPDbContext.DefaultSchema);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NombreComercial)
            .HasMaxLength(200);

        builder.Property(x => x.DocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Direccion)
            .HasMaxLength(250);

        builder.Property(x => x.Telefono)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(254);

        builder.Property(x => x.Activa)
            .HasDefaultValue(true);

        builder.Property(x => x.CreadaEnUtc)
            .IsRequired();

        builder.Property(x => x.ActualizadaEnUtc);

        builder.HasIndex(x => x.DocumentoIdentidad)
            .IsUnique();
    }
}
