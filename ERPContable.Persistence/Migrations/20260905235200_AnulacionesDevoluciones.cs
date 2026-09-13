using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AnulacionesDevoluciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoInventario",
                schema: "erp",
                table: "VentasDetalle",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoUnitarioInventario",
                schema: "erp",
                table: "VentasDetalle",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CorreccionesDocumentos",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    CompraId = table.Column<int>(type: "integer", nullable: true),
                    VentaId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreadoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UsuarioNombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uuid", nullable: false),
                    Completa = table.Column<bool>(type: "boolean", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Igv = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AsientoContableId = table.Column<int>(type: "integer", nullable: true),
                    AsientoCostoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreccionesDocumentos", x => x.Id);
                    table.CheckConstraint("CK_Correccion_Documento", "(\"CompraId\" IS NULL) <> (\"VentaId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_CorreccionesDocumentos_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalSchema: "erp",
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDocumentos_AsientosContables_AsientoCostoId",
                        column: x => x.AsientoCostoId,
                        principalSchema: "erp",
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDocumentos_Compras_CompraId",
                        column: x => x.CompraId,
                        principalSchema: "erp",
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDocumentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDocumentos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "erp",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorreccionesDetalles",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CorreccionDocumentoId = table.Column<int>(type: "integer", nullable: false),
                    CompraDetalleId = table.Column<int>(type: "integer", nullable: true),
                    VentaDetalleId = table.Column<int>(type: "integer", nullable: true),
                    Cantidad = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoInventario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MovimientoInventarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreccionesDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorreccionesDetalles_ComprasDetalle_CompraDetalleId",
                        column: x => x.CompraDetalleId,
                        principalSchema: "erp",
                        principalTable: "ComprasDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDetalles_CorreccionesDocumentos_CorreccionDocum~",
                        column: x => x.CorreccionDocumentoId,
                        principalSchema: "erp",
                        principalTable: "CorreccionesDocumentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDetalles_MovimientosInventario_MovimientoInvent~",
                        column: x => x.MovimientoInventarioId,
                        principalSchema: "erp",
                        principalTable: "MovimientosInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorreccionesDetalles_VentasDetalle_VentaDetalleId",
                        column: x => x.VentaDetalleId,
                        principalSchema: "erp",
                        principalTable: "VentasDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDetalles_CompraDetalleId",
                schema: "erp",
                table: "CorreccionesDetalles",
                column: "CompraDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDetalles_CorreccionDocumentoId",
                schema: "erp",
                table: "CorreccionesDetalles",
                column: "CorreccionDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDetalles_MovimientoInventarioId",
                schema: "erp",
                table: "CorreccionesDetalles",
                column: "MovimientoInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDetalles_VentaDetalleId",
                schema: "erp",
                table: "CorreccionesDetalles",
                column: "VentaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDocumentos_AsientoContableId",
                schema: "erp",
                table: "CorreccionesDocumentos",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDocumentos_AsientoCostoId",
                schema: "erp",
                table: "CorreccionesDocumentos",
                column: "AsientoCostoId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDocumentos_CompraId",
                schema: "erp",
                table: "CorreccionesDocumentos",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDocumentos_EmpresaId_SolicitudId",
                schema: "erp",
                table: "CorreccionesDocumentos",
                columns: new[] { "EmpresaId", "SolicitudId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesDocumentos_VentaId",
                schema: "erp",
                table: "CorreccionesDocumentos",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorreccionesDetalles",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "CorreccionesDocumentos",
                schema: "erp");

            migrationBuilder.DropColumn(
                name: "CostoInventario",
                schema: "erp",
                table: "VentasDetalle");

            migrationBuilder.DropColumn(
                name: "CostoUnitarioInventario",
                schema: "erp",
                table: "VentasDetalle");
        }
    }
}
