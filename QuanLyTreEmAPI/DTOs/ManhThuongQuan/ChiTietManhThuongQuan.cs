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
        public List<PhieuMinhChungDto> PhieuMinhChung { get; set; }
    }

    public class PhieuMinhChungDto
    {
        public int PhieuMinhChungID { get; set; }
        public string LoaiMinhChung { get; set; }
        public string FilePath { get; set; }
        public DateTime? NgayCap { get; set; }
    }
}
