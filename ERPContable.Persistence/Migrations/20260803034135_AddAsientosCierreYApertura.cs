using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAsientosCierreYApertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsientoAperturaId",
                schema: "erp",
                table: "CierresEjercicios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AsientoCierreId",
                schema: "erp",
                table: "CierresEjercicios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfiguracionesContablesEmpresas",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    CuentaResultadoAcumuladoId = table.Column<int>(type: "integer", nullable: false),
                    ActualizadoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesContablesEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContablesEmpresas_CuentasContables_CuentaRes~",
                        column: x => x.CuentaResultadoAcumuladoId,
                        principalSchema: "erp",
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContablesEmpresas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContablesEmpresas_CuentaResultadoAcumuladoId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                column: "CuentaResultadoAcumuladoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContablesEmpresas_EmpresaId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                column: "EmpresaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesContablesEmpresas",
                schema: "erp");

            migrationBuilder.DropColumn(
                name: "AsientoAperturaId",
                schema: "erp",
                table: "CierresEjercicios");

            migrationBuilder.DropColumn(
                name: "AsientoCierreId",
                schema: "erp",
                table: "CierresEjercicios");
        }
    }
}
