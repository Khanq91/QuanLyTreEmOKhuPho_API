namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class ThongKeTinhNguyenVienDTO
    {
        public int? KhuPhoID { get; set; }

        public int? UserID { get; set; }
        public int TinhNguyenVienID { get; set; }
        public string TenTinhNguyenVien { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string TenKhuPho { get; set; }
        public string SDT { get; set; }
        public string ChucVu { get; set; }
        public int SoLuongCaRanh { get; set; }
        public int TongSuKienThamGia { get; set; }
    }
}
