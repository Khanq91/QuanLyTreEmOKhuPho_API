namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class PhieuHocTapDTO
    {
        public int? PhieuHocTapID { get; set; }
        public double? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public string HanhKiem { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string TenTruong { get; set; }
        public string TenLop { get; set; }
        public int? TruongID { get; set; }
        public int? LopID { get; set; }
    }
}
