namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class TreEmPhanPhatQuaDTO
    {
        // Thông tin phân phát
        public int PhanPhatID { get; set; }

        // Thông tin trẻ em
        public int? TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string? Anh { get; set; }

        // Thông tin quà tặng
        public int? QuaTangUngHoID { get; set; }
        public string TenQua { get; set; }
        public string MoTaQua { get; set; }
        public string? AnhQua { get; set; }

        // Thông tin sự kiện
        public int SuKienID { get; set; }
        public string TenSuKien { get; set; }

        // Thông tin phân phát
        public int SoLuongNhan { get; set; }
        public DateOnly NgayPhanPhat { get; set; }
        public string NguoiPhanPhat { get; set; }
        public string TrangThai { get; set; } // "Đã nhận", "Đang tiến hành"
        public string? GhiChu { get; set; }

        // Thông tin khu phố
        public string TenKhuPho { get; set; }
    }
}
