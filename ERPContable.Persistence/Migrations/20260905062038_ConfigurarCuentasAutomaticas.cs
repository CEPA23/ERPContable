using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarCuentasAutomaticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuentaCostoVentasId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaInventarioId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE erp."ConfiguracionesContablesEmpresas" AS config
                SET "CuentaInventarioId" = inventory."Id",
                    "CuentaCostoVentasId" = cost."Id"
                FROM erp."CuentasContables" AS inventory
                INNER JOIN erp."CuentasContables" AS cost
                    ON cost."EmpresaId" = inventory."EmpresaId"
                   AND cost."Codigo" = '69'
                WHERE inventory."EmpresaId" = config."EmpresaId"
                  AND inventory."Codigo" = '20';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContablesEmpresas_CuentaCostoVentasId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                column: "CuentaCostoVentasId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContablesEmpresas_CuentaInventarioId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                column: "CuentaInventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContablesEmpresas_CuentasContables_CuentaCos~",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                column: "CuentaCostoVentasId",
                principalSchema: "erp",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContablesEmpresas_CuentasContables_CuentaInv~",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas",
                column: "CuentaInventarioId",
                principalSchema: "erp",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContablesEmpresas_CuentasContables_CuentaCos~",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContablesEmpresas_CuentasContables_CuentaInv~",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContablesEmpresas_CuentaCostoVentasId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContablesEmpresas_CuentaInventarioId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas");

            migrationBuilder.DropColumn(
                name: "CuentaCostoVentasId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas");

            migrationBuilder.DropColumn(
                name: "CuentaInventarioId",
                schema: "erp",
                table: "ConfiguracionesContablesEmpresas");
        }
    }
}
