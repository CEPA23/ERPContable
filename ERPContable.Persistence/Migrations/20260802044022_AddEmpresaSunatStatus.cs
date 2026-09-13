using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaSunatStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CondicionSunat",
                schema: "erp",
                table: "Empresas",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoSunat",
                schema: "erp",
                table: "Empresas",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CondicionSunat",
                schema: "erp",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "EstadoSunat",
                schema: "erp",
                table: "Empresas");
        }
    }
}
