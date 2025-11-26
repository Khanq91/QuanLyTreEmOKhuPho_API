namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class LichSuPhanPhatQuaDTO
    {
        public int PhanPhatID { get; set; }
        public string TenQua { get; set; }
        public string TenSuKien { get; set; }
        public int SoLuongNhan { get; set; }
        public DateOnly NgayPhanPhat { get; set; }
        public string TrangThai { get; set; }
        public string? GhiChu { get; set; }
        public string? AnhQua { get; set; }
    }
}
