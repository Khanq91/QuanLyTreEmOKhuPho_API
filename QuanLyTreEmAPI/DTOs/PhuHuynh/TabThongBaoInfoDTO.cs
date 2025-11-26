namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class TabThongBaoInfoDTO
    {
        public int ThongBaoID { get; set; }
        public string NoiDung { get; set; }
        public string NgayThongBao { get; set; }
        public bool DaDoc { get; set; }
        public int? SuKienID { get; set; }
        public string TenSuKien { get; set; }
    }
}
