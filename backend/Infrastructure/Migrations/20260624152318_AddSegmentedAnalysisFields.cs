using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSegmentedAnalysisFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryDisplayName",
                table: "DocumentAnalyses",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryKey",
                table: "DocumentAnalyses",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Confidence",
                table: "DocumentAnalyses",
                type: "TEXT",
                precision: 5,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSegmented",
                table: "DocumentAnalyses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MainFieldsJson",
                table: "DocumentAnalyses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SegmentsJson",
                table: "DocumentAnalyses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedActionsJson",
                table: "DocumentAnalyses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryDisplayName",
                table: "DocumentAnalyses");

            migrationBuilder.DropColumn(
                name: "CategoryKey",
                table: "DocumentAnalyses");

            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "DocumentAnalyses");

            migrationBuilder.DropColumn(
                name: "IsSegmented",
                table: "DocumentAnalyses");

            migrationBuilder.DropColumn(
                name: "MainFieldsJson",
                table: "DocumentAnalyses");

            migrationBuilder.DropColumn(
                name: "SegmentsJson",
                table: "DocumentAnalyses");

            migrationBuilder.DropColumn(
                name: "SuggestedActionsJson",
                table: "DocumentAnalyses");
        }
    }
}
