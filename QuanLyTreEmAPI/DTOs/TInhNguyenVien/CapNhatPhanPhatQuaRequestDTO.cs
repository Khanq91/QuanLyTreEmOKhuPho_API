namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class CapNhatPhanPhatQuaRequestDTO
    {
        public int PhanPhatID { get; set; }
        public string TrangThai { get; set; } // "Đã nhận" hoặc "Đang tiến hành"
        public DateOnly NgayPhanPhat { get; set; }
        public string? GhiChu { get; set; }
    }
}
