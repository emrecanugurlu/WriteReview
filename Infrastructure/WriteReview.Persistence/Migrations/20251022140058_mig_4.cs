using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WriteReview.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleReview_Articles_ArticleId",
                table: "ArticleReview");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleReview_AspNetUsers_ReviewerId",
                table: "ArticleReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleReview",
                table: "ArticleReview");

            migrationBuilder.RenameTable(
                name: "ArticleReview",
                newName: "ArticleReviews");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleReview_ReviewerId",
                table: "ArticleReviews",
                newName: "IX_ArticleReviews_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleReview_ArticleId",
                table: "ArticleReviews",
                newName: "IX_ArticleReviews_ArticleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleReviews",
                table: "ArticleReviews",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleReviews_Articles_ArticleId",
                table: "ArticleReviews",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleReviews_AspNetUsers_ReviewerId",
                table: "ArticleReviews",
                column: "ReviewerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleReviews_Articles_ArticleId",
                table: "ArticleReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleReviews_AspNetUsers_ReviewerId",
                table: "ArticleReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleReviews",
                table: "ArticleReviews");

            migrationBuilder.RenameTable(
                name: "ArticleReviews",
                newName: "ArticleReview");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleReviews_ReviewerId",
                table: "ArticleReview",
                newName: "IX_ArticleReview_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleReviews_ArticleId",
                table: "ArticleReview",
                newName: "IX_ArticleReview_ArticleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleReview",
                table: "ArticleReview",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleReview_Articles_ArticleId",
                table: "ArticleReview",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleReview_AspNetUsers_ReviewerId",
                table: "ArticleReview",
                column: "ReviewerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
