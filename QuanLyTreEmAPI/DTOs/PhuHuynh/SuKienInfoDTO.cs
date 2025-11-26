namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class SuKienInfoDTO
    {
        public int SuKienID { get; set; }
        public string TenSuKien { get; set; }
        public string MoTa { get; set; }
        public string DiaDiem { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public string TenKhuPho { get; set; }
    }
}
