namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class ThongBaoInfoDTO
    {
        public int ThongBaoID { get; set; }
        public string NoiDung { get; set; }
        public DateOnly? NgayThongBao { get; set; }
        public string TenSuKien { get; set; }
    }
}
