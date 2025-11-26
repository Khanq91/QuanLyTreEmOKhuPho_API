namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class SuKienPhanCongDTO
    {
        public int? SuKienId { get; set; }
        public string TenSuKien { get; set; }
        public string CongViec { get; set; }
        public DateOnly NgayPhanCong { get; set; }
        public DateOnly NgayBatDau { get; set; }
        public DateOnly NgayKetThuc { get; set; }
        public string TrangThai { get; set; } // "Sắp diễn ra", "Đang diễn ra", "Đã kết thúc"
        public string DiaDiem { get; set; }
    }
}
