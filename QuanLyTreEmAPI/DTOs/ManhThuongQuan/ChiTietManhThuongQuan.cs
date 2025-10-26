namespace QuanLyTreEmAPI.DTOs.ManhThuongQuan
{
    public class ChiTietManhThuongQuan
    {

        public int ManhThuongQuanID { get; set; }
        public string Ten { get; set; }
        public string Loai { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTienUngHo { get; set; }
        public int SoLanUngHo { get; set; }

        public  string? NgayUngHoGanNhat { get; set; }
    }
}
