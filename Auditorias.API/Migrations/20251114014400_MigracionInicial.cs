using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditorias.API.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Responsable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AreaAuditada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.CheckConstraint("CK_Auditorias_Estado", "[Estado] IN ('Pendiente', 'En Proceso', 'Finalizada')");
                    table.ForeignKey(
                        name: "FK_Auditorias_Responsable_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Responsable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hallazgos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaDeteccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Severidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AuditoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hallazgos", x => x.Id);
                    table.CheckConstraint("CK_Hallazgos_Severidad", "[Severidad] IN ('Baja', 'Media', 'Alta')");
                    table.CheckConstraint("CK_Hallazgos_Tipo", "[Tipo] IN ('Observacion', 'No Conformidad')");
                    table.ForeignKey(
                        name: "FK_Hallazgos_Auditorias_AuditoriaId",
                        column: x => x.AuditoriaId,
                        principalTable: "Auditorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_ResponsableId",
                table: "Auditorias",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Hallazgos_AuditoriaId",
                table: "Hallazgos",
                column: "AuditoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hallazgos");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Responsable");
        }
    }
}
