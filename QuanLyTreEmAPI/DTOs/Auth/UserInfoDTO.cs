namespace QuanLyTreEmAPI.DTOs.Auth
{
    public class UserInfoDTO
    {
        public int UserID { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string VaiTro { get; set; }
        public int? KhuPhoID { get; set; }
        public string TenKhuPho { get; set; }
    }
}
