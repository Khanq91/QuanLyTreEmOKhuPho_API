using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTreEmAPI.Models
{
    public class SuKienDTOCreate
    {
        [Column("SuKienID")]

        public int SuKienId { get; set; }

        [Required(ErrorMessage = "Tên sự kiện là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sự kiện không được vượt quá 200 ký tự")]
        public string TenSuKien { get; set; } = null!;

        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        [StringLength(200)]
        public string DiaDiem { get; set; } = null!;

        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }

        public string? NguoiChiuTrachNhiem { get; set; }
        public string? AnhSuKien { get; set; }
        public int? SoLuongTinhNguyenVien { get; set; }
        public int? SoLuongTreEm { get; set; }

        [Required(ErrorMessage = "UserId là bắt buộc")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "KhuPhoId là bắt buộc")]
        public int KhuPhoId { get; set; }

        // === CHI TIẾT ===
        public List<ThoiGianChiTietDTO> ThoiGianChiTiet { get; set; } = new();
        public List<TietMucDTO> TietMuc { get; set; } = new();
        public List<ChiPhiSuKienDTO> ChiPhi { get; set; } = new();
        public List<PhanCongDTO> PhanCong { get; set; } = new();
    }

    // Thời gian chi tiết + tiết mục
    public class ThoiGianChiTietDTO
    {
        public int ThoiGianChiTietId { get; set; }       // thêm ID
        public string? MoTa { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public List<TietMucDTO> TietMuc { get; set; } = new();

    }

    public class TietMucDTO
    {
        public int TietMucId { get; set; }               // 🔹 thêm ID
        [Required]
        public string TenTietMuc { get; set; } = null!;
        public string? NguoiThucHien { get; set; }
        public int? ThoiGianChiTietSuKienId { get; set; }
        public decimal? ChiPhiTietMuc { get; set; }
    }

    // Chi phí + chi tiết chi phí
    public class ChiPhiSuKienDTO
    {
        public int ChiPhiId { get; set; }                // 🔹 sửa tên cho đúng casing
        public string TenKhoanChi { get; set; } = null!;
        public decimal? SoTien { get; set; }
        public string? GhiChu { get; set; }
        public List<ChiTietChiPhiDTO> ChiTiet { get; set; } = new();
    }

    public class ChiTietChiPhiDTO
    {
        public int ChiTietId { get; set; }               // 🔹 sửa lại ID chính
        public int ChiPhiId { get; set; }                // FK
        public string TenPhanQua { get; set; } = null!;
        public string? NguoiDaiDien { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
    }

    // Phân công tình nguyện viên
    public class PhanCongDTO
    {
        public int PhanCongId { get; set; }              // thêm ID
        [Required(ErrorMessage = "TinhNguyenVienId là bắt buộc")]
        public int TinhNguyenVienId { get; set; }

        [Required]
        public string CongViec { get; set; } = null!;
        public string? GhiChu { get; set; }
    }
}
