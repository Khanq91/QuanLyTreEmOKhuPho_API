namespace QuanLyTreEmAPI.DTOs.TInhNguyenVien
{
    public class HoTroPhucLoiDaPhatDTO
    {
        public int HoTroID { get; set; }
        public string LoaiHoTro { get; set; }
        public string MoTa { get; set; }
        public DateOnly? NgayCap { get; set; }
        public string TenTreEm { get; set; }
        public string TenKhuPho { get; set; }
        public string TrangThaiPhat { get; set; }

    }
}
