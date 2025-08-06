using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactHUB.Migrations
{
    /// <inheritdoc />
    public partial class AddRecuperacionOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etiqueta_Estado_Id_Estado",
                table: "Etiqueta");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Estado",
                table: "Etiqueta",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "RecuperacionOtps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecuperacionOtps", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Etiqueta_Estado_Id_Estado",
                table: "Etiqueta",
                column: "Id_Estado",
                principalTable: "Estado",
                principalColumn: "IdEstado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etiqueta_Estado_Id_Estado",
                table: "Etiqueta");

            migrationBuilder.DropTable(
                name: "RecuperacionOtps");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Estado",
                table: "Etiqueta",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Etiqueta_Estado_Id_Estado",
                table: "Etiqueta",
                column: "Id_Estado",
                principalTable: "Estado",
                principalColumn: "IdEstado",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
