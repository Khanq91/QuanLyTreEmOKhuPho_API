namespace QuanLyTreEmAPI.DTOs.ManhThuongQuan
{
    public class UngHoDTO
    {
        public int UngHoId { get; set; }
        public decimal? SoTien { get; set; }
        public string? LoaiUngHo { get; set; }
        public string? NgayUngHo { get; set; } // Trả về string để dễ nhận phía client
        public string? GhiChu { get; set; }
        public string? TenManhThuongQuan { get; set; }
        public int ManhThuongQuanId { get; set; }

    }
}
