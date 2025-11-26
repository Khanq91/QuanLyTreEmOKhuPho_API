namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class DanhSachKhuPhoDTO
    {
        public int KhuPhoId { get; set; }
        public string TenKhuPho { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? QuanHuyen { get; set; }
        public string? ThanhPho { get; set; }
    }
}
