namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class CapNhatThongTinPhuHuynhRequestDTO
    {
        public int PhuHuynhID { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }
        //public string Email { get; set; }
        public string DiaChi { get; set; }
        public string NgheNghiep { get; set; }
        public string NgaySinh { get; set; } // dd/MM/yyyy
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
    }
}
