namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class PhieuHocTapInfoDTO
    {
        public int PhieuHocTapID { get; set; }
        public double? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public string HanhKiem { get; set; }
        public string NhanXet { get; set; } // GhiChu
        public string NgayCapNhat { get; set; } // dd/MM/yyyy
        public string TenLop { get; set; }
        public string TenTruong { get; set; }
    }
}
