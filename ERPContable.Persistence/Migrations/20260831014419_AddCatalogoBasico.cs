using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoBasico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasProductos",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreadaEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasProductos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Impuestos",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Tasa = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Impuestos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Abreviatura = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesMedida_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductosServicios",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: true),
                    UnidadMedidaId = table.Column<int>(type: "integer", nullable: false),
                    ImpuestoId = table.Column<int>(type: "integer", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoReferencial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosServicios_CategoriasProductos_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "erp",
                        principalTable: "CategoriasProductos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductosServicios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "erp",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductosServicios_Impuestos_ImpuestoId",
                        column: x => x.ImpuestoId,
                        principalSchema: "erp",
                        principalTable: "Impuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosServicios_UnidadesMedida_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalSchema: "erp",
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProductos_EmpresaId_Nombre",
                schema: "erp",
                table: "CategoriasProductos",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Impuestos_EmpresaId_Codigo",
                schema: "erp",
                table: "Impuestos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosServicios_CategoriaId",
                schema: "erp",
                table: "ProductosServicios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosServicios_EmpresaId_Codigo",
                schema: "erp",
                table: "ProductosServicios",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosServicios_ImpuestoId",
                schema: "erp",
                table: "ProductosServicios",
                column: "ImpuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosServicios_UnidadMedidaId",
                schema: "erp",
                table: "ProductosServicios",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_EmpresaId_Codigo",
                schema: "erp",
                table: "UnidadesMedida",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductosServicios",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "CategoriasProductos",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "Impuestos",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "UnidadesMedida",
                schema: "erp");
        }
    }
}
