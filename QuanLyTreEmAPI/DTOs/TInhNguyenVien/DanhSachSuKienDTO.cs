namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class DanhSachSuKienDTO
    {
        public int SuKienId { get; set; }
        public string TenSuKien { get; set; }
        public string? MoTa { get; set; }
        public string DiaDiem { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public int? SoLuongTinhNguyenVien { get; set; }
        public int? SoLuongTreEm { get; set; }
        public string NguoiChiuTrachNhiem { get; set; }
        public string TenKhuPho { get; set; }
        public int? DangKyId { get; set; }
        public string? TrangThaiDangKy { get; set; }
        public string? CongViecPhanCong { get; set; }
        public bool DaPhanCong { get; set; }
        public int SoLuongDaDangKy { get; set; }
        public string? AnhSuKien { get; set; }

    }
}
