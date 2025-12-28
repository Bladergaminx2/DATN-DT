using DATN_DT.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_DT.DTO
{
    public class DiaChiDTO
    {
        public int Id { get; set; }
        public int IdKhachHang { get; set; }
        public string Tennguoinhan { get; set; }
        public string sdtnguoinhan { get; set; }

        // CODE (DB)
        public string Thanhpho { get; set; }     // province_id
        public string Quanhuyen { get; set; }    // district_id
        public string Phuongxa { get; set; }     // ward_code
        public string? Diachicuthe { get; set; }
        public int trangthai { get; set; }

        // NAME (new)
        public string? ThanhphoName { get; set; }
        public string? QuanhuyenName { get; set; }
        public string? PhuongxaName { get; set; }
    }


}
