namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    public class DanhSachUngHo
    {
        public int HoTroID { get; set; }
        public string LoaiUngHo { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayUngHo { get; set; }
        public string TenManhThuongQuan { get; set; }
        public string TenKhuPho { get; set; }
        public string DiaChiKhuPho { get; set; }
        public string QuanHuyen { get; set; }
        public string ThanhPho { get; set; }
        public decimal SoTien { get; set; }
        public int SoLuongTreEmDuocUngHo { get; set; }
        public int TreDaNhan { get; set; }
        public int TreChuaNhan { get; set; }
            
        public int PercentDaPhat => SoLuongTreEmDuocUngHo > 0
      ? (int)((TreDaNhan / (double)SoLuongTreEmDuocUngHo) * 100)
      : 0;
    }
}
