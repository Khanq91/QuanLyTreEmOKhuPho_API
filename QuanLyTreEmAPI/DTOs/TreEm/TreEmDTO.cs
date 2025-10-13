namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class TreEmDTO
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public int Tuoi { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
        public string TinhTrang { get; set; }
        public string TenTruong { get; set; }
        public string TenKhuPho { get; set; }
        public string MucUuTien { get; set; }
        public List<string> HoanCanh { get; set; }
        public List<PhuHuynhSimpleDTO> PhuHuynh { get; set; }
    }
}
