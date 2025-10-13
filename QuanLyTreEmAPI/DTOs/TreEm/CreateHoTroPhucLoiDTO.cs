namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class CreateHoTroPhucLoiDTO
    {
        public int TreEmID { get; set; }
        public string LoaiHoTro { get; set; }
        public string MoTa { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NguoiChiuTrachNhiemHoTro { get; set; }
    }
}
