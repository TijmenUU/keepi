using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keepi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IdentityOrigin = table.Column<int>(type: "INTEGER", nullable: false),
                    EntriesPermission = table.Column<int>(type: "INTEGER", nullable: false),
                    ExportsPermission = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectsPermission = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersPermission = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEntityUserEntity",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEntityUserEntity", x => new { x.ProjectsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ProjectEntityUserEntity_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectEntityUserEntity_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEntries_InvoiceItems_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalTable: "InvoiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInvoiceItemCustomizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvoiceItemCustomizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInvoiceItemCustomizations_InvoiceItems_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalTable: "InvoiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInvoiceItemCustomizations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_Name_ProjectId",
                table: "InvoiceItems",
                columns: new[] { "Name", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ProjectId",
                table: "InvoiceItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEntityUserEntity_UsersId",
                table: "ProjectEntityUserEntity",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEntries_Date",
                table: "UserEntries",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntries_InvoiceItemId",
                table: "UserEntries",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntries_UserId",
                table: "UserEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvoiceItemCustomizations_InvoiceItemId",
                table: "UserInvoiceItemCustomizations",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvoiceItemCustomizations_UserId_InvoiceItemId",
                table: "UserInvoiceItemCustomizations",
                columns: new[] { "UserId", "InvoiceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityOrigin_ExternalId",
                table: "Users",
                columns: new[] { "IdentityOrigin", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectEntityUserEntity");

            migrationBuilder.DropTable(
                name: "UserEntries");

            migrationBuilder.DropTable(
                name: "UserInvoiceItemCustomizations");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
