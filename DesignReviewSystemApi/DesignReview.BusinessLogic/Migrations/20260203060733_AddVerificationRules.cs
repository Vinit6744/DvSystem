using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesignReview.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VerificationRuleId",
                table: "VerificationResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VerificationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SearchPatterns = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidationPattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationResults_VerificationRuleId",
                table: "VerificationResults",
                column: "VerificationRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationResults_VerificationRules_VerificationRuleId",
                table: "VerificationResults",
                column: "VerificationRuleId",
                principalTable: "VerificationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VerificationResults_VerificationRules_VerificationRuleId",
                table: "VerificationResults");

            migrationBuilder.DropTable(
                name: "VerificationRules");

            migrationBuilder.DropIndex(
                name: "IX_VerificationResults_VerificationRuleId",
                table: "VerificationResults");

            migrationBuilder.DropColumn(
                name: "VerificationRuleId",
                table: "VerificationResults");
        }
    }
}
