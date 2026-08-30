using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveApprovedByRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Users_ApprovedByUserId",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_ApprovedByUserId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Leaves");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Leaves",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ApprovedBy",
                table: "Leaves",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_UserId",
                table: "Leaves",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Users_ApprovedBy",
                table: "Leaves",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Users_UserId",
                table: "Leaves",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Users_ApprovedBy",
                table: "Leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Users_UserId",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_ApprovedBy",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_UserId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Leaves");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Leaves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ApprovedByUserId",
                table: "Leaves",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Users_ApprovedByUserId",
                table: "Leaves",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
