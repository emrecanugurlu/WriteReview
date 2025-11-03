using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WriteReview.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleExpertAssignments",
                table: "ArticleExpertAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ArticleExpertAssignments_ArticleId",
                table: "ArticleExpertAssignments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArticleExpertAssignments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleExpertAssignments",
                table: "ArticleExpertAssignments",
                columns: new[] { "ArticleId", "ExpertId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleExpertAssignments",
                table: "ArticleExpertAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ArticleExpertAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleExpertAssignments",
                table: "ArticleExpertAssignments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleExpertAssignments_ArticleId",
                table: "ArticleExpertAssignments",
                column: "ArticleId");
        }
    }
}
