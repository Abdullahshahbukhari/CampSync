using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRoomNumberPerCamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_CampId",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CampId_RoomNo",
                table: "Rooms",
                columns: new[] { "CampId", "RoomNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_CampId_RoomNo",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CampId",
                table: "Rooms",
                column: "CampId");
        }
    }
}
