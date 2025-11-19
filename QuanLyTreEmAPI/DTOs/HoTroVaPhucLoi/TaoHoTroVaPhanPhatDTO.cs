namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    // ===================== REQUEST DTO =====================
    public class TaoHoTroVaPhanPhatDTO
    {
        // ✅ THÔNG TIN ỦNG HỘ
        public int UngHoId { get; set; }
        public string? TenDotHoTro { get; set; } // ← Thêm thuộc tính này (nếu cần)

        // ✅ THÔNG TIN QUÀ TẶNG
        public int? SuKienId { get; set; }
        public string? TenQua { get; set; }
        public string? MoTaQua { get; set; }
        public string? DoiTuongNhan { get; set; }
        public string? AnhQua { get; set; }

        // ⚡ MỚI: cho phép chọn quà tặng có sẵn hoặc gửi đơn giá
        public int? QuaTangUngHoId { get; set; }
        public decimal? DonGia { get; set; }

        // ✅ THÔNG TIN HỖ TRỢ PHÚC LỢI
        public string LoaiHoTro { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateOnly NgayCap { get; set; }
        public string? NguoiChiuTrachNhiemHoTro { get; set; }
        public string? TrangThaiPhat { get; set; }
        public DateOnly? NgayHenLai { get; set; }
        public string? GhiChuTNV { get; set; }
        public int NguoiDungID { get; set; }

        // ✅ THÔNG TIN PHÂN PHÁT
        public DateOnly NgayPhanPhat { get; set; }
        public string? NguoiPhanPhat { get; set; }
        public string? GhiChuPhanPhat { get; set; }

        // ✅ DANH SÁCH TRẺ EM NHẬN
        public List<TreEmNhanQuaDTO> DanhSachTreEmNhan { get; set; } = new();
    }

    // ===================== TRẺ EM NHẬN QUÀ DTO =====================
    public class TreEmNhanQuaDTO
    {
        public int TreEmId { get; set; }
        public int SoLuongNhan { get; set; } = 1;
        public string? GhiChu { get; set; }
    }

    // ===================== RESPONSE DTO =====================
    public class TaoHoTroVaPhanPhatResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UngHoId { get; set; }
        public int QuaTangUngHoId { get; set; }
        public int SoTreEmDaNhan { get; set; }
        public int TongSoLuongPhat { get; set; }
        public int SoLuongConLaiQuaTang { get; set; }
        public int SoLuongConLaiUngHo { get; set; }
        public List<HoTroPhucLoiInfo> DanhSachHoTroPhucLoi { get; set; } = new();
        public List<PhanPhatQuaInfo> DanhSachPhanPhat { get; set; } = new();
    }

    // ===================== HỖ TRỢ PHÚC LỢI INFO =====================
    public class HoTroPhucLoiInfo
    {
        public int HoTroId { get; set; }
        public int TreEmId { get; set; }
        public string HoTenTreEm { get; set; } = string.Empty;
        public string LoaiHoTro { get; set; } = string.Empty;
        public string TrangThaiPhat { get; set; } = string.Empty;
    }

    // ===================== PHÂN PHÁT QUÀ INFO =====================
    public class PhanPhatQuaInfo
    {
        public int PhanPhatId { get; set; }
        public int TreEmId { get; set; }
        public string HoTenTreEm { get; set; } = string.Empty;
        public int SoLuongNhan { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}