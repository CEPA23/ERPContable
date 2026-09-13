using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCierresContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CierresEjercicios",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Ejercicio = table.Column<int>(type: "integer", nullable: false),
                    Cerrado = table.Column<bool>(type: "boolean", nullable: false),
                    CerradoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CerradoPorUsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReabiertoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReabiertoPorUsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresEjercicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CierresPeriodos",
                schema: "erp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Ejercicio = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Cerrado = table.Column<bool>(type: "boolean", nullable: false),
                    CerradoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CerradoPorUsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReabiertoEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReabiertoPorUsuarioId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresPeriodos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CierresEjercicios_EmpresaId_Ejercicio",
                schema: "erp",
                table: "CierresEjercicios",
                columns: new[] { "EmpresaId", "Ejercicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CierresPeriodos_EmpresaId_Ejercicio_Mes",
                schema: "erp",
                table: "CierresPeriodos",
                columns: new[] { "EmpresaId", "Ejercicio", "Mes" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CierresEjercicios",
                schema: "erp");

            migrationBuilder.DropTable(
                name: "CierresPeriodos",
                schema: "erp");
        }
    }
}
