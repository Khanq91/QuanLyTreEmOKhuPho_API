namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class UpdateProfileRequestDTO
    {
        public string? HoTen { get; set; }
        public string? Sdt { get; set; }
        public string? Email { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public int? KhuPhoId { get; set; }
        public string? TenKhuPhoMoi { get; set; }
        public string? DiaChiKhuPho { get; set; }
        public string? QuanHuyen { get; set; }
        public string? ThanhPho { get; set; }
    }
}
