using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBaoHanhSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm ThoiHanBaoHanh vào ModelSanPham
            migrationBuilder.AddColumn<int>(
                name: "ThoiHanBaoHanh",
                table: "ModelSanPhams",
                type: "int",
                nullable: true);

            // Thêm LoaiBaoHanh vào BaoHanh
            migrationBuilder.AddColumn<string>(
                name: "LoaiBaoHanh",
                table: "BaoHanhs",
                type: "nvarchar(max)",
                nullable: true);

            // Tạo bảng BaoHanhLichSus
            migrationBuilder.CreateTable(
                name: "BaoHanhLichSus",
                columns: table => new
                {
                    IdBaoHanhLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdBaoHanh = table.Column<int>(type: "int", nullable: false),
                    IdNhanVien = table.Column<int>(type: "int", nullable: true),
                    ThaoTac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiCu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiMoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoHanhLichSus", x => x.IdBaoHanhLichSu);
                    table.ForeignKey(
                        name: "FK_BaoHanhLichSus_BaoHanhs_IdBaoHanh",
                        column: x => x.IdBaoHanh,
                        principalTable: "BaoHanhs",
                        principalColumn: "IdBaoHanh",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoHanhLichSus_NhanViens_IdNhanVien",
                        column: x => x.IdNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "IdNhanVien",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhLichSus_IdBaoHanh",
                table: "BaoHanhLichSus",
                column: "IdBaoHanh");

            migrationBuilder.CreateIndex(
                name: "IX_BaoHanhLichSus_IdNhanVien",
                table: "BaoHanhLichSus",
                column: "IdNhanVien");

            // Cập nhật giá trị mặc định cho ThoiHanBaoHanh = 12 tháng
            migrationBuilder.Sql("UPDATE ModelSanPhams SET ThoiHanBaoHanh = 12 WHERE ThoiHanBaoHanh IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoHanhLichSus");

            migrationBuilder.DropColumn(
                name: "ThoiHanBaoHanh",
                table: "ModelSanPhams");

            migrationBuilder.DropColumn(
                name: "LoaiBaoHanh",
                table: "BaoHanhs");
        }
    }
}

