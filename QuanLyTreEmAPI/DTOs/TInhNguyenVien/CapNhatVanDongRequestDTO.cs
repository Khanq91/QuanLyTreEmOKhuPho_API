namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class CapNhatVanDongRequestDTO
    {
        public int TreEmID { get; set; }
        public int HoanCanhID { get; set; }
        public string TinhTrangCapNhat { get; set; } // 'Đi học', 'Nghỉ học', 'Nguy cơ bỏ học', 'Khác'
        public string? GhiChuChiTiet { get; set; }
        public int SoLan { get; set; }
    }
}
