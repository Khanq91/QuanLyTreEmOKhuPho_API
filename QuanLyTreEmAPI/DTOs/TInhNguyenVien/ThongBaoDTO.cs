namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ThongBaoDTO
    {
        public int ThongBaoId { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateOnly? NgayThongBao { get; set; }
        public bool DaDoc { get; set; }
        public int? SuKienId { get; set; }
        public string? TenSuKien { get; set; }

        /// <summary>
        /// Loại thông báo: SuKien, VanDong, HoTro, PhanCong, ThongTin
        /// Xác định từ nội dung
        /// </summary>
        public string LoaiThongBao { get; set; } = "ThongTin";

        /// <summary>
        /// Mức độ ưu tiên: URGENT, IMPORTANT, INFO
        /// Xác định từ từ khóa trong nội dung
        /// </summary>
        public string MucDoUuTien { get; set; } = "INFO";
    }
}
