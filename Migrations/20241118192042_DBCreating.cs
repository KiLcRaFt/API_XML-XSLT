using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_XML_XSLT.Migrations
{
    /// <inheritdoc />
    public partial class DBCreating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tootajad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nimi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Perenimi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefoni_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Salasyna = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Is_admin = table.Column<bool>(type: "bit", nullable: false),
                    Amet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tootajad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IgapaevaAndmed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TootajaId = table.Column<int>(type: "int", nullable: false),
                    Kuupaev = table.Column<DateOnly>(type: "date", nullable: false),
                    Too_algus = table.Column<TimeOnly>(type: "time", nullable: true),
                    Too_lypp = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgapaevaAndmed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IgapaevaAndmed_Tootajad_TootajaId",
                        column: x => x.TootajaId,
                        principalTable: "Tootajad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IgapaevaAndmed_TootajaId",
                table: "IgapaevaAndmed",
                column: "TootajaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IgapaevaAndmed");

            migrationBuilder.DropTable(
                name: "Tootajad");
        }
    }
}
