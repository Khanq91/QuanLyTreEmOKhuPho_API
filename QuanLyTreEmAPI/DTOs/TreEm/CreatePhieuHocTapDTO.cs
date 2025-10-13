namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class CreatePhieuHocTapDTO
    {
        public int TreEmID { get; set; }
        public double? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public string HanhKiem { get; set; }
        public string GhiChu { get; set; }
        public int? TruongID { get; set; }
        public int? LopID { get; set; }
    }
}
