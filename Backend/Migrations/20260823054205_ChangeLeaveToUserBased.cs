using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLeaveToUserBased : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Workers_WorkerId",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "Leaves",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Leaves_WorkerId",
                table: "Leaves",
                newName: "IX_Leaves_UserId");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Users_UserId",
                table: "Leaves",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Users_UserId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Leaves",
                newName: "WorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_Leaves_UserId",
                table: "Leaves",
                newName: "IX_Leaves_WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Workers_WorkerId",
                table: "Leaves",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
