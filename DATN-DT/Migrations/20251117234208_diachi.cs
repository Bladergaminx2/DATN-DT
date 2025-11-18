using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class diachi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "diachis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdKhachHang = table.Column<int>(type: "int", nullable: false),
                    Tennguoinhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sdtnguoinhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Thanhpho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quanhuyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phuongxa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Diachicuthe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trangthai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diachis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_diachis_KhachHangs_IdKhachHang",
                        column: x => x.IdKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKhachHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_diachis_IdKhachHang",
                table: "diachis",
                column: "IdKhachHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diachis");
        }
    }
}
