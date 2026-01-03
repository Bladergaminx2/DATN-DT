using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class voucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    IdVoucher = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVoucher = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenVoucher = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiGiam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonHangToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GiamToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongSuDung = table.Column<int>(type: "int", nullable: true),
                    SoLuongDaSuDung = table.Column<int>(type: "int", nullable: false),
                    SoLuongMoiKhachHang = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApDungCho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanhSachId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.IdVoucher);
                });

            migrationBuilder.CreateTable(
                name: "VoucherSuDungs",
                columns: table => new
                {
                    IdVoucherSuDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVoucher = table.Column<int>(type: "int", nullable: false),
                    IdKhachHang = table.Column<int>(type: "int", nullable: false),
                    IdHoaDon = table.Column<int>(type: "int", nullable: true),
                    SoTienGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgaySuDung = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherSuDungs", x => x.IdVoucherSuDung);
                    table.ForeignKey(
                        name: "FK_VoucherSuDungs_HoaDon_IdHoaDon",
                        column: x => x.IdHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "IdHoaDon",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VoucherSuDungs_KhachHangs_IdKhachHang",
                        column: x => x.IdKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherSuDungs_Vouchers_IdVoucher",
                        column: x => x.IdVoucher,
                        principalTable: "Vouchers",
                        principalColumn: "IdVoucher",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherSuDungs_IdHoaDon",
                table: "VoucherSuDungs",
                column: "IdHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherSuDungs_IdKhachHang",
                table: "VoucherSuDungs",
                column: "IdKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherSuDungs_IdVoucher",
                table: "VoucherSuDungs",
                column: "IdVoucher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoucherSuDungs");

            migrationBuilder.DropTable(
                name: "Vouchers");
        }
    }
}
