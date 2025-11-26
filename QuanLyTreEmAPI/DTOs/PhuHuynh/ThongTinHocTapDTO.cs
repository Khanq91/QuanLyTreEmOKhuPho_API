namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class ThongTinHocTapDTO
    {
        public int PhieuHocTapID { get; set; }
        public string TenTreEm { get; set; }
        public string TenTruong { get; set; }
        public string TenLop { get; set; }
        public double? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public string HanhKiem { get; set; }
        public string GhiChu { get; set; }
        public DateOnly? NamHoc { get; set; }
    }
}
