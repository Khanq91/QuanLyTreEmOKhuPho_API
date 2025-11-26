namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class ThongTinTreEmChiTietResponseDTO
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
        public string Anh { get; set; }
        public int? TruongID { get; set; }
        public string TenTruong { get; set; }
        public string CapHoc { get; set; }
        public int? LopID { get; set; }
        public string TenLop { get; set; }
    }
}
