using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class capnhatchucvu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TrangThaiNV",
                table: "NhanViens",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenChucVuVietHoa",
                table: "ChucVus",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenChucVuVietHoa",
                table: "ChucVus");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThaiNV",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
