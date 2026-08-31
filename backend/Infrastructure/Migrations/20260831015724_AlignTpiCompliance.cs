using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignTpiCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (IsSqlite(migrationBuilder))
            {
                ApplySqlite(migrationBuilder);
                return;
            }

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "LicenseDocumentUrl",
                table: "LawyerProfiles",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ChatId",
                table: "Documents",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PromptVersion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audits_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId",
                unique: true,
                filter: "Status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_ChatId",
                table: "Audits",
                column: "ChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LicenseDocumentUrl",
                table: "LawyerProfiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomSkills");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChatId",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");
        }

        private static bool IsSqlite(MigrationBuilder migrationBuilder)
            => string.Equals(
                migrationBuilder.ActiveProvider,
                "Microsoft.EntityFrameworkCore.Sqlite",
                StringComparison.Ordinal);

        /// <summary>
        /// SQLite no soporta ADD COLUMN IF NOT EXISTS. En bases locales IsActive nunca
        /// se llegó a borrar, y un intento previo de esta migración pudo quedar a medias.
        /// </summary>
        private static void ApplySqlite(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Subscriptions_UserId";
                """, suppressTransaction: true);

            migrationBuilder.Sql("""
                PRAGMA foreign_keys = OFF;
                BEGIN;

                CREATE TABLE "ef_tmp_CustomSkills" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_CustomSkills" PRIMARY KEY,
                    "LawyerProfileId" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "WhenToUse" TEXT NOT NULL,
                    "Instructions" TEXT NOT NULL,
                    "Examples" TEXT NOT NULL,
                    "RedFlags" TEXT NOT NULL,
                    "OutputFormat" TEXT NOT NULL,
                    "IsActive" INTEGER NOT NULL DEFAULT 1,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_CustomSkills_LawyerProfiles_LawyerProfileId" FOREIGN KEY ("LawyerProfileId") REFERENCES "LawyerProfiles" ("Id") ON DELETE CASCADE
                );
                INSERT INTO "ef_tmp_CustomSkills" ("Id", "LawyerProfileId", "Name", "WhenToUse", "Instructions", "Examples", "RedFlags", "OutputFormat", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT "Id", "LawyerProfileId", "Name", "WhenToUse", "Instructions", "Examples", "RedFlags", "OutputFormat", 1, "CreatedAt", "UpdatedAt"
                FROM "CustomSkills";
                DROP TABLE "CustomSkills";
                ALTER TABLE "ef_tmp_CustomSkills" RENAME TO "CustomSkills";
                CREATE INDEX "IX_CustomSkills_LawyerProfileId" ON "CustomSkills" ("LawyerProfileId");

                CREATE TABLE "ef_tmp_LawyerProfiles" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_LawyerProfiles" PRIMARY KEY,
                    "UserId" TEXT NOT NULL,
                    "LicenseNumber" TEXT NOT NULL,
                    "BarAssociation" TEXT NOT NULL,
                    "Province" TEXT NOT NULL,
                    "Specialty" TEXT NOT NULL,
                    "IsVerified" INTEGER NOT NULL,
                    "VerifiedById" TEXT NULL,
                    "VerifiedAt" TEXT NULL,
                    "VerificationStatus" TEXT NOT NULL,
                    "RejectionReason" TEXT NULL,
                    "ResolvedAt" TEXT NULL,
                    "LicenseDocumentUrl" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_LawyerProfiles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
                INSERT INTO "ef_tmp_LawyerProfiles" ("Id", "UserId", "LicenseNumber", "BarAssociation", "Province", "Specialty", "IsVerified", "VerifiedById", "VerifiedAt", "VerificationStatus", "RejectionReason", "ResolvedAt", "LicenseDocumentUrl", "CreatedAt", "UpdatedAt")
                SELECT "Id", "UserId", "LicenseNumber", "BarAssociation", "Province", "Specialty", "IsVerified", "VerifiedById", "VerifiedAt", "VerificationStatus", "RejectionReason", "ResolvedAt", NULL, "CreatedAt", "UpdatedAt"
                FROM "LawyerProfiles";
                DROP TABLE "LawyerProfiles";
                ALTER TABLE "ef_tmp_LawyerProfiles" RENAME TO "LawyerProfiles";
                CREATE UNIQUE INDEX "IX_LawyerProfiles_UserId" ON "LawyerProfiles" ("UserId");

                CREATE TABLE "ef_tmp_Documents" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_Documents" PRIMARY KEY,
                    "ChatId" TEXT NULL,
                    "FolderId" TEXT NULL,
                    "Title" TEXT NOT NULL,
                    "Url" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_Documents_Chats_ChatId" FOREIGN KEY ("ChatId") REFERENCES "Chats" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_Documents_Folders_FolderId" FOREIGN KEY ("FolderId") REFERENCES "Folders" ("Id") ON DELETE SET NULL
                );
                INSERT INTO "ef_tmp_Documents" ("Id", "ChatId", "FolderId", "Title", "Url", "CreatedAt", "UpdatedAt")
                SELECT "Id", "ChatId", "FolderId", "Title", "Url", "CreatedAt", "UpdatedAt"
                FROM "Documents";
                DROP TABLE "Documents";
                ALTER TABLE "ef_tmp_Documents" RENAME TO "Documents";
                CREATE INDEX "IX_Documents_ChatId" ON "Documents" ("ChatId");
                CREATE INDEX "IX_Documents_FolderId" ON "Documents" ("FolderId");

                CREATE TABLE IF NOT EXISTS "Audits" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_Audits" PRIMARY KEY,
                    "ChatId" TEXT NOT NULL,
                    "Model" TEXT NOT NULL,
                    "PromptVersion" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    "UpdatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_Audits_Chats_ChatId" FOREIGN KEY ("ChatId") REFERENCES "Chats" ("Id") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Audits_ChatId" ON "Audits" ("ChatId");

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Subscriptions_UserId" ON "Subscriptions" ("UserId") WHERE Status = 'Active';

                COMMIT;
                PRAGMA foreign_keys = ON;
                """, suppressTransaction: true);
        }
    }
}
