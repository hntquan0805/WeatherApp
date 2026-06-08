using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace WeatherApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'user'"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "weatherlog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    Temperature = table.Column<decimal>(type: "decimal(5,2)", precision: 5, nullable: false, comment: "Đơn vị: °C"),
                    FeelsLike = table.Column<decimal>(type: "decimal(5,2)", precision: 5, nullable: false, comment: "Đơn vị: °C"),
                    Humidity = table.Column<sbyte>(type: "tinyint", nullable: false, comment: "Đơn vị: %  (0 - 100)"),
                    WindSpeed = table.Column<decimal>(type: "decimal(6,2)", precision: 6, nullable: false, comment: "Đơn vị: m/s"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    SearchedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherLog_User",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IDX_User_Email",
                table: "user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IDX_User_Id",
                table: "user",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IDX_User_Username",
                table: "user",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "UQ_User_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_User_Username",
                table: "user",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_WeatherLog_Id",
                table: "weatherlog",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IDX_WeatherLog_UserId",
                table: "weatherlog",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weatherlog");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
