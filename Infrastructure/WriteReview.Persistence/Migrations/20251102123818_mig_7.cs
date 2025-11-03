using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WriteReview.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserExpertiseArea");

            migrationBuilder.CreateTable(
                name: "UserExpertiseArea",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpertiseAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExpertiseArea", x => new { x.UserId, x.ExpertiseAreaId });
                    table.ForeignKey(
                        name: "FK_UserExpertiseArea_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserExpertiseArea_ExpertiseAreas_ExpertiseAreaId",
                        column: x => x.ExpertiseAreaId,
                        principalTable: "ExpertiseAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserExpertiseArea_ExpertiseAreaId",
                table: "UserExpertiseArea",
                column: "ExpertiseAreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExpertiseArea");

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
    }
}
