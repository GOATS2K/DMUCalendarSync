using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMUCalendarSync.Database.Migrations
{
    public partial class initialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyDmuUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    Surname = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDmuUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MyDmuCookieSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MyDmuUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    EarliestCookieExpiry = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDmuCookieSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyDmuCookieSets_MyDmuUsers_MyDmuUserId",
                        column: x => x.MyDmuUserId,
                        principalTable: "MyDmuUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyDmuCookies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MyDmuUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MyDmuCookieSetId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDmuCookies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyDmuCookies_MyDmuCookieSets_MyDmuCookieSetId",
                        column: x => x.MyDmuCookieSetId,
                        principalTable: "MyDmuCookieSets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MyDmuCookies_MyDmuUsers_MyDmuUserId",
                        column: x => x.MyDmuUserId,
                        principalTable: "MyDmuUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyDmuCookies_MyDmuCookieSetId",
                table: "MyDmuCookies",
                column: "MyDmuCookieSetId");

            migrationBuilder.CreateIndex(
                name: "IX_MyDmuCookies_MyDmuUserId",
                table: "MyDmuCookies",
                column: "MyDmuUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MyDmuCookieSets_MyDmuUserId",
                table: "MyDmuCookieSets",
                column: "MyDmuUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyDmuCookies");

            migrationBuilder.DropTable(
                name: "MyDmuCookieSets");

            migrationBuilder.DropTable(
                name: "MyDmuUsers");
        }
    }
}
