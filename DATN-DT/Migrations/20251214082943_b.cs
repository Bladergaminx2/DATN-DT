using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdNhanVien",
                table: "DonHangs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_IdNhanVien",
                table: "DonHangs",
                column: "IdNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_NhanViens_IdNhanVien",
                table: "DonHangs",
                column: "IdNhanVien",
                principalTable: "NhanViens",
                principalColumn: "IdNhanVien",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_NhanViens_IdNhanVien",
                table: "DonHangs");

            migrationBuilder.DropIndex(
                name: "IX_DonHangs_IdNhanVien",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "IdNhanVien",
                table: "DonHangs");
        }
    }
}
