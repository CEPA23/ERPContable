using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventarioProductos",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ProductoServicioId = table.Column<int>(type: "integer", nullable: false),
                    StockActual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoPromedio = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioProductos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventarioProductos_ProductosServicios_ProductoServicioId",
                        column: x => x.ProductoServicioId,
                        principalSchema: "erp",
                        principalTable: "ProductosServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ProductoServicioId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StockAnterior = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StockPosterior = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Referencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_ProductosServicios_ProductoServicioId",
                        column: x => x.ProductoServicioId,
                        principalSchema: "erp",
                        principalTable: "ProductosServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioProductos_EmpresaId_ProductoServicioId",
                schema: "erp",
                table: "InventarioProductos",
                columns: new[] { "EmpresaId", "ProductoServicioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventarioProductos_ProductoServicioId",
                schema: "erp",
                table: "InventarioProductos",
                column: "ProductoServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_FechaUtc",
                schema: "erp",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoServicioId",
                schema: "erp",
                table: "MovimientosInventario",
                column: "ProductoServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventarioProductos",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "MovimientosInventario",
                schema: "erp");
        }
    }
}
