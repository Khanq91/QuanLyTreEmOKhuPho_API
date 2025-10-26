namespace QuanLyTreEmAPI.DTOs.ManhThuongQuan
{
    public class ThongTinUngHo
    {
        public int? ManhThuongQuanId { get; set; }
        public decimal? SoTien { get; set; }
        public DateOnly NgayUngHo { get; set; }
        public string? HinhThuc { get; set; }
        public string? GhiChu { get; set; }
    }
}
