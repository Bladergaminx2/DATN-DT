using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_DT.Migrations
{
    /// <inheritdoc />
    public partial class addkhuyenmaisp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelSanPhamKhuyenMais",
                columns: table => new
                {
                    IdModelSanPhamKhuyenMai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdModelSanPham = table.Column<int>(type: "int", nullable: true),
                    IdKhuyenMai = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelSanPhamKhuyenMais", x => x.IdModelSanPhamKhuyenMai);
                    table.ForeignKey(
                        name: "FK_ModelSanPhamKhuyenMais_KhuyenMais_IdKhuyenMai",
                        column: x => x.IdKhuyenMai,
                        principalTable: "KhuyenMais",
                        principalColumn: "IdKhuyenMai",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModelSanPhamKhuyenMais_ModelSanPhams_IdModelSanPham",
                        column: x => x.IdModelSanPham,
                        principalTable: "ModelSanPhams",
                        principalColumn: "IdModelSanPham",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhamKhuyenMais_IdKhuyenMai",
                table: "ModelSanPhamKhuyenMais",
                column: "IdKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_ModelSanPhamKhuyenMais_IdModelSanPham",
                table: "ModelSanPhamKhuyenMais",
                column: "IdModelSanPham");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelSanPhamKhuyenMais");
        }
    }
}
