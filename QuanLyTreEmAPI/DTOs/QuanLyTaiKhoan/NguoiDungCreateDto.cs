namespace QuanLyTreEmAPI.DTOs.QuanLyTaiKhoan
{
    public class NguoiDungCreateDto
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string VaiTro { get; set; }
        public string MatKhau { get; set; }
        public string TrangThai { get; set; }
        public string Anh { get; set; }
        public DateOnly NgayTao { get; set; }
    }
}
