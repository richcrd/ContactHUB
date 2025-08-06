using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactHUB.Migrations
{
    public partial class RolTableAndUsuarioFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar columna Rol antigua
            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Usuario");

            // Crear tabla Rol
            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.IdRol);
                });

            // Insertar roles iniciales
            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "IdRol", "Nombre" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Usuario" }
                });

            // Agregar columna IdRol a Usuario (por defecto 2 = Usuario)
            migrationBuilder.AddColumn<int>(
                name: "IdRol",
                table: "Usuario",
                nullable: false,
                defaultValue: 2);

            // Actualizar usuarios existentes a rol Usuario
            migrationBuilder.Sql("UPDATE Usuario SET IdRol = 2 WHERE IdRol = 0");

            // Crear índice y FK
            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario",
                column: "IdRol");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Rol_IdRol",
                table: "Usuario",
                column: "IdRol",
                principalTable: "Rol",
                principalColumn: "IdRol",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Rol_IdRol",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "IdRol",
                table: "Usuario");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
