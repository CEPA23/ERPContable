using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanContablePorEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CuentasContables_Codigo",
                schema: "erp",
                table: "CuentasContables");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                schema: "erp",
                table: "CuentasContables",
                type: "integer",
                nullable: true);

            // Las cuentas existentes eran globales. Se conservan como plantillas y se
            // crean copias por empresa; luego cada referencia histórica apunta a su copia.
            migrationBuilder.Sql("""
                INSERT INTO erp."CuentasContables" ("EmpresaId", "Codigo", "Nombre", "Tipo", "Activa")
                SELECT e."Id", c."Codigo", c."Nombre", c."Tipo", c."Activa"
                FROM erp."Empresas" e CROSS JOIN erp."CuentasContables" c
                WHERE c."EmpresaId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE erp."DetallesAsiento" d SET "CuentaContableId" = destino."Id"
                FROM erp."AsientosContables" a, erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE d."AsientoContableId" = a."Id" AND d."CuentaContableId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = a."EmpresaId" AND destino."Codigo" = origen."Codigo";
                """);
            migrationBuilder.Sql("""
                UPDATE erp."Compras" p SET "CuentaContrapartidaId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE p."CuentaContrapartidaId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = p."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."Compras" p SET "CuentaIgvId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE p."CuentaIgvId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = p."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."ComprasDetalle" d SET "CuentaContableId" = destino."Id"
                FROM erp."Compras" p, erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE d."CompraId" = p."Id" AND d."CuentaContableId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = p."EmpresaId" AND destino."Codigo" = origen."Codigo";
                """);
            migrationBuilder.Sql("""
                UPDATE erp."Ventas" v SET "CuentaContrapartidaId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE v."CuentaContrapartidaId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = v."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."Ventas" v SET "CuentaIgvId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE v."CuentaIgvId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = v."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."VentasDetalle" d SET "CuentaContableId" = destino."Id"
                FROM erp."Ventas" v, erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE d."VentaId" = v."Id" AND d."CuentaContableId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = v."EmpresaId" AND destino."Codigo" = origen."Codigo";
                """);
            migrationBuilder.Sql("""
                UPDATE erp."MovimientosCaja" m SET "CuentaCajaId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE m."CuentaCajaId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = m."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."MovimientosCaja" m SET "CuentaContrapartidaId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE m."CuentaContrapartidaId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = m."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."CuentasBancarias" b SET "CuentaContableId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE b."CuentaContableId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = b."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."MovimientosBanco" m SET "CuentaContrapartidaId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE m."CuentaContrapartidaId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = m."EmpresaId" AND destino."Codigo" = origen."Codigo";
                """);
            migrationBuilder.Sql("""
                UPDATE erp."ActivosFijos" a SET "CuentaActivoId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE a."CuentaActivoId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = a."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."ActivosFijos" a SET "CuentaDepreciacionId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE a."CuentaDepreciacionId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = a."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."ActivosFijos" a SET "CuentaGastoId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE a."CuentaGastoId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = a."EmpresaId" AND destino."Codigo" = origen."Codigo";
                UPDATE erp."ConfiguracionesContablesEmpresas" c SET "CuentaResultadoAcumuladoId" = destino."Id"
                FROM erp."CuentasContables" origen, erp."CuentasContables" destino
                WHERE c."CuentaResultadoAcumuladoId" = origen."Id" AND origen."EmpresaId" IS NULL
                  AND destino."EmpresaId" = c."EmpresaId" AND destino."Codigo" = origen."Codigo";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_EmpresaId_Codigo",
                schema: "erp",
                table: "CuentasContables",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasContables_Empresas_EmpresaId",
                schema: "erp",
                table: "CuentasContables",
                column: "EmpresaId",
                principalSchema: "erp",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentasContables_Empresas_EmpresaId",
                schema: "erp",
                table: "CuentasContables");

            migrationBuilder.DropIndex(
                name: "IX_CuentasContables_EmpresaId_Codigo",
                schema: "erp",
                table: "CuentasContables");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                schema: "erp",
                table: "CuentasContables");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_Codigo",
                schema: "erp",
                table: "CuentasContables",
                column: "Codigo",
                unique: true);
        }
    }
}
