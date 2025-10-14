namespace QuanLyTreEmAPI.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string HoTen { get; set; }
        public string VaiTro { get; set; }
        public int UserID { get; set; }
    }
}
