namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class CapNhatHoTroRequestDTO
    {
        public int HoTroID { get; set; }
        public string TrangThaiPhat { get; set; } // 'Đã phát thành công', 'Chưa nhận', 'Khác'
        public DateOnly? NgayHenLai { get; set; } // Nếu trạng thái là 'Chưa nhận'
        public string? GhiChuTNV { get; set; }
    }
}
