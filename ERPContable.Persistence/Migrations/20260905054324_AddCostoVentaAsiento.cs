using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostoVentaAsiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsientoCostoVentaId",
                schema: "erp",
                table: "Ventas",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsientoCostoVentaId",
                schema: "erp",
                table: "Ventas");
        }
    }
}
