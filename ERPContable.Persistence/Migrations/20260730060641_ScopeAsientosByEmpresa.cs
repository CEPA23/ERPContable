using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeAsientosByEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                schema: "erp",
                table: "AsientosContables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId",
                schema: "erp",
                table: "AsientosContables",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_Empresas_EmpresaId",
                schema: "erp",
                table: "AsientosContables",
                column: "EmpresaId",
                principalSchema: "erp",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_Empresas_EmpresaId",
                schema: "erp",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_EmpresaId",
                schema: "erp",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                schema: "erp",
                table: "AsientosContables");
        }
    }
}
