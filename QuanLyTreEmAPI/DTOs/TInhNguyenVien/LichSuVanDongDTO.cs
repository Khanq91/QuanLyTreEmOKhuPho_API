namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class LichSuVanDongDTO
    {
        public int VanDongID { get; set; }
        public string TinhTrangCapNhat { get; set; }
        public string GhiChuChiTiet { get; set; }
        public string? AnhMinhChung { get; set; }
        public DateOnly? NgayCapNhat { get; set; }
        public int? SoLan { get; set; }
    }
}
