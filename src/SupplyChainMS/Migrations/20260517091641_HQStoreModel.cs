using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyChainMS.Migrations
{
    /// <inheritdoc />
    public partial class HQStoreModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_ManagerUserId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ManagerUserId",
                table: "Stores");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerUserId",
                table: "Stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ManagerUserId",
                table: "Stores",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_ManagerUserId",
                table: "Stores",
                column: "ManagerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_ManagerUserId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ManagerUserId",
                table: "Stores");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerUserId",
                table: "Stores",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ManagerUserId",
                table: "Stores",
                column: "ManagerUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_ManagerUserId",
                table: "Stores",
                column: "ManagerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
