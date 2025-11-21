namespace QuanLyTreEmAPI.DTOs.HoTroVaPhucLoi
{
    public class ThemTreVaoPhanPhatDTO
    {
        public int QuaTangUngHoId { get; set; }
        public List<TreNhanDTO> DanhSachTreEm { get; set; }
        public DateTime NgayPhat { get; set; }
        public string NguoiPhat { get; set; }
    }

    public class TreNhanDTO
    {
        public int TreEmId { get; set; }
        public int SoLuongNhan { get; set; }
        public string GhiChu { get; set; }
    }
}
