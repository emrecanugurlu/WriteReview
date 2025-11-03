using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WriteReview.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpertiseAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpertiseAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserExpertiseArea",
                columns: table => new
                {
                    ExpertiseAreasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserExpertiseArea", x => new { x.ExpertiseAreasId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AppUserExpertiseArea_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserExpertiseArea_ExpertiseAreas_ExpertiseAreasId",
                        column: x => x.ExpertiseAreasId,
                        principalTable: "ExpertiseAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserExpertiseArea_UsersId",
                table: "AppUserExpertiseArea",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserExpertiseArea");

            migrationBuilder.DropTable(
                name: "ExpertiseAreas");
        }
    }
}
