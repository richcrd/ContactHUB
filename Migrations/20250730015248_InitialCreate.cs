// Archivo eliminado para limpiar migraciones previas
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactHUB.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estado",
                columns: table => new
                {
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estado", x => x.IdEstado);
                });

            migrationBuilder.CreateTable(
                name: "Departamento",
                columns: table => new
                {
                    IdDepartamento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamento", x => x.IdDepartamento);
                    table.ForeignKey(
                        name: "FK_Departamento_Estado_Id_Estado",
                        column: x => x.Id_Estado,
                        principalTable: "Estado",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Etiqueta",
                columns: table => new
                {
                    IdEtiqueta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etiqueta", x => x.IdEtiqueta);
                    table.ForeignKey(
                        name: "FK_Etiqueta_Estado_Id_Estado",
                        column: x => x.Id_Estado,
                        principalTable: "Estado",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Estado_Id_Estado",
                        column: x => x.Id_Estado,
                        principalTable: "Estado",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacto",
                columns: table => new
                {
                    IdContacto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_Departamento = table.Column<int>(type: "int", nullable: true),
                    Id_Estado = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacto", x => x.IdContacto);
                    table.ForeignKey(
                        name: "FK_Contacto_Departamento_Id_Departamento",
                        column: x => x.Id_Departamento,
                        principalTable: "Departamento",
                        principalColumn: "IdDepartamento");
                    table.ForeignKey(
                        name: "FK_Contacto_Estado_Id_Estado",
                        column: x => x.Id_Estado,
                        principalTable: "Estado",
                        principalColumn: "IdEstado");
                    table.ForeignKey(
                        name: "FK_Contacto_Usuario_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactoEtiqueta",
                columns: table => new
                {
                    IdContacto = table.Column<int>(type: "int", nullable: false),
                    IdEtiqueta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactoEtiqueta", x => new { x.IdContacto, x.IdEtiqueta });
                    table.ForeignKey(
                        name: "FK_ContactoEtiqueta_Contacto_IdContacto",
                        column: x => x.IdContacto,
                        principalTable: "Contacto",
                        principalColumn: "IdContacto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactoEtiqueta_Etiqueta_IdEtiqueta",
                        column: x => x.IdEtiqueta,
                        principalTable: "Etiqueta",
                        principalColumn: "IdEtiqueta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Id_Departamento",
                table: "Contacto",
                column: "Id_Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Id_Estado",
                table: "Contacto",
                column: "Id_Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Id_Usuario",
                table: "Contacto",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_ContactoEtiqueta_IdEtiqueta",
                table: "ContactoEtiqueta",
                column: "IdEtiqueta");

            migrationBuilder.CreateIndex(
                name: "IX_Departamento_Id_Estado",
                table: "Departamento",
                column: "Id_Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Etiqueta_Id_Estado",
                table: "Etiqueta",
                column: "Id_Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Id_Estado",
                table: "Usuario",
                column: "Id_Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactoEtiqueta");

            migrationBuilder.DropTable(
                name: "Contacto");

            migrationBuilder.DropTable(
                name: "Etiqueta");

            migrationBuilder.DropTable(
                name: "Departamento");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Estado");
        }
    }
}
