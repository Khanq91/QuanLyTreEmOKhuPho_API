namespace QuanLyTreEmAPI.DTOs.ManhThuongQuan
{
    public class LichSuUng
    {
        public int STT { get; set; }
        public string NgayUngHo { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public string Loai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }
}
