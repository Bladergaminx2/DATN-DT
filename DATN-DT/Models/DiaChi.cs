using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_DT.Models
{
    public class DiaChi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int IdKhachHang { get; set; }
        [ForeignKey("IdKhachHang")]
        public virtual KhachHang KhachHang { get; set; }

        public string Tennguoinhan { get; set; }

        public string sdtnguoinhan { get; set; }

        public string Thanhpho { get; set; }

        public string Quanhuyen { get; set; }

        public string Phuongxa { get; set; }

        public string? Diachicuthe { get; set; }

        public int trangthai { get; set; }
    }
}