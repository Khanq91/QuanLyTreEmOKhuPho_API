namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class MoiQuanHeVoiTreEmDTO
    {
        public int TreEmID { get; set; }
        public string TenTreEm { get; set; } = string.Empty;
        public string MoiQuanHe { get; set; } = string.Empty; // Cha, Mẹ, Ông, Bà...
        public string Anh { get; set; } = string.Empty;
    }
}
