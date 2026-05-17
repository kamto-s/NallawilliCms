using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nallawilli.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateSectionCodePerPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CmsSections_PageId_SectionCode",
                table: "CmsSections");

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_PageId",
                table: "CmsSections",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CmsSections_PageId",
                table: "CmsSections");

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_PageId_SectionCode",
                table: "CmsSections",
                columns: new[] { "PageId", "SectionCode" },
                unique: true);
        }
    }
}
