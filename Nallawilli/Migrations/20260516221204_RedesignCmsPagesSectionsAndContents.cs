using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nallawilli.Migrations
{
    /// <inheritdoc />
    public partial class RedesignCmsPagesSectionsAndContents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CmsSections_PageId",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "PropertyBag",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "CmsPages");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "CmsPages");

            migrationBuilder.RenameColumn(
                name: "MenuOrder",
                table: "CmsPages",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "CmsPages",
                newName: "IsActive");

            migrationBuilder.AddColumn<string>(
                name: "SectionCode",
                table: "CmsSections",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "CmsSections",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CmsPages",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "CmsPages",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "CmsPages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "CmsPages",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CmsSectionContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContentValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InputType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsSectionContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsSectionContents_CmsSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "CmsSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_PageId_SectionCode",
                table: "CmsSections",
                columns: new[] { "PageId", "SectionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsSectionContents_SectionId_ContentKey",
                table: "CmsSectionContents",
                columns: new[] { "SectionId", "ContentKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsSectionContents");

            migrationBuilder.DropIndex(
                name: "IX_CmsSections_PageId_SectionCode",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "SectionCode",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "CmsSections");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "CmsPages");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "CmsPages");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "CmsPages",
                newName: "MenuOrder");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "CmsPages",
                newName: "IsPublished");

            migrationBuilder.AddColumn<string>(
                name: "PropertyBag",
                table: "CmsSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CmsSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "CmsSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CmsPages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "CmsPages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "CmsPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "CmsPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_PageId",
                table: "CmsSections",
                column: "PageId");
        }
    }
}
