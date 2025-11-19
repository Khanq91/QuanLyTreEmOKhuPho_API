namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    public class DanhSachUngHoRaw
    {
        public int HoTroID { get; set; }
        public string LoaiUngHo { get; set; }
        public string MoTa { get; set; }
        public DateOnly? NgayUngHo { get; set; }
        public string TenManhThuongQuan { get; set; }
        public decimal SoTien { get; set; }
        public int SoLuongTreEmDuocUngHo { get; set; }
        public int TreDaNhan { get; set; }
        public string TenKhuPho { get; set; }
        public string DiaChiKhuPho { get; set; }
        public string QuanHuyen { get; set; }
        public string ThanhPho { get; set; }
    }
}
