using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPContable.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AsientoFechaSinZonaHoraria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                schema: "erp",
                table: "AsientosContables",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                schema: "erp",
                table: "AsientosContables",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
