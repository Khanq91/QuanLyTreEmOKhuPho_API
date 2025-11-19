namespace QuanLyTreEmAPI.DTOs.ManhThuongQuan
{
    public class ThongTinUngHo
    {
        public int? ManhThuongQuanId { get; set; }
        public decimal? SoTien { get; set; }
        public int SoLuongVatPham { get; set; }
        public DateOnly NgayUngHo { get; set; }
        public string? DoiTuong { get; set; }
        public string? TenVatPham { get; set; }
        public string? LoaiUngHo { get; set; }
        public string? GhiChu { get; set; }
    }
}
