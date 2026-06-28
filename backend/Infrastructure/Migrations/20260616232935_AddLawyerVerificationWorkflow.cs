using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLawyerVerificationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "LawyerProfiles",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "LawyerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LawyerVerificationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LawyerProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerVerificationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerVerificationDocuments_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LawyerVerificationDocuments_LawyerProfileId",
                table: "LawyerVerificationDocuments",
                column: "LawyerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawyerVerificationDocuments");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "LawyerProfiles");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "LawyerProfiles");
        }
    }
}
