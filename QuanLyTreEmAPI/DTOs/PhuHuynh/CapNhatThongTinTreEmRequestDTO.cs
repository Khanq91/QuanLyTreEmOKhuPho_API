namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class CapNhatThongTinTreEmRequestDTO
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; } // dd/MM/yyyy
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
        public int? TruongID { get; set; }
        public int? LopID { get; set; }
    }
}
