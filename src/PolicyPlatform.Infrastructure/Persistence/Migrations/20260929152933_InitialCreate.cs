using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PolicyholderName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LineOfBusiness = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Underwriter = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FlaggedForReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_EffectiveDate",
                table: "Policies",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ExpiryDate",
                table: "Policies",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_FlaggedForReview",
                table: "Policies",
                column: "FlaggedForReview");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Region",
                table: "Policies",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Status_LineOfBusiness",
                table: "Policies",
                columns: new[] { "Status", "LineOfBusiness" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
