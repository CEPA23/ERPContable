using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventarioVinculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductoServicioId",
                schema: "erp",
                table: "VentasDetalle",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductoServicioId",
                schema: "erp",
                table: "ComprasDetalle",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalle_ProductoServicioId",
                schema: "erp",
                table: "VentasDetalle",
                column: "ProductoServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasDetalle_ProductoServicioId",
                schema: "erp",
                table: "ComprasDetalle",
                column: "ProductoServicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalle_ProductosServicios_ProductoServicioId",
                schema: "erp",
                table: "ComprasDetalle",
                column: "ProductoServicioId",
                principalSchema: "erp",
                principalTable: "ProductosServicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VentasDetalle_ProductosServicios_ProductoServicioId",
                schema: "erp",
                table: "VentasDetalle",
                column: "ProductoServicioId",
                principalSchema: "erp",
                principalTable: "ProductosServicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalle_ProductosServicios_ProductoServicioId",
                schema: "erp",
                table: "ComprasDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasDetalle_ProductosServicios_ProductoServicioId",
                schema: "erp",
                table: "VentasDetalle");

            migrationBuilder.DropIndex(
                name: "IX_VentasDetalle_ProductoServicioId",
                schema: "erp",
                table: "VentasDetalle");

            migrationBuilder.DropIndex(
                name: "IX_ComprasDetalle_ProductoServicioId",
                schema: "erp",
                table: "ComprasDetalle");

            migrationBuilder.DropColumn(
                name: "ProductoServicioId",
                schema: "erp",
                table: "VentasDetalle");

            migrationBuilder.DropColumn(
                name: "ProductoServicioId",
                schema: "erp",
                table: "ComprasDetalle");
        }
    }
}
