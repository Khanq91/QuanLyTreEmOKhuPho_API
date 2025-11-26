namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class TreEmDaVanDongDTO
    {
        public int VanDongID { get; set; }
        public string TenTreEm { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TenKhuPho { get; set; }
        public string LoaiHoanCanh { get; set; }
        public int? SoLan { get; set; }
        public string LyDo { get; set; }
        public string KetQua { get; set; }
        public DateOnly? NgayVanDong { get; set; }
        public string TinhTrangCapNhat { get; set; }
        public string GhiChuChiTiet { get; set; }
    }
}
