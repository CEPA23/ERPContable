using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBancos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuentasBancarias",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Banco = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NumeroCuenta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CuentaContableId = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "erp",
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosBanco",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    CuentaBancariaId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CuentaContrapartidaId = table.Column<int>(type: "integer", nullable: false),
                    AsientoContableId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosBanco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosBanco_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalSchema: "erp",
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_CuentaContableId",
                schema: "erp",
                table: "CuentasBancarias",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_EmpresaId_NumeroCuenta",
                schema: "erp",
                table: "CuentasBancarias",
                columns: new[] { "EmpresaId", "NumeroCuenta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBanco_CuentaBancariaId",
                schema: "erp",
                table: "MovimientosBanco",
                column: "CuentaBancariaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosBanco",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "CuentasBancarias",
                schema: "erp");
        }
    }
}
