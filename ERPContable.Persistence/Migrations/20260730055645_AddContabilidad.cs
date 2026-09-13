using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsientosContables",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Glosa = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreadoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosContables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentasContables",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasContables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesAsiento",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AsientoContableId = table.Column<int>(type: "integer", nullable: false),
                    CuentaContableId = table.Column<int>(type: "integer", nullable: false),
                    Debe = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Haber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesAsiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesAsiento_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalSchema: "erp",
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesAsiento_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "erp",
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_Codigo",
                schema: "erp",
                table: "CuentasContables",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesAsiento_AsientoContableId",
                schema: "erp",
                table: "DetallesAsiento",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesAsiento_CuentaContableId",
                schema: "erp",
                table: "DetallesAsiento",
                column: "CuentaContableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesAsiento",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "AsientosContables",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "CuentasContables",
                schema: "erp");
        }
    }
}
