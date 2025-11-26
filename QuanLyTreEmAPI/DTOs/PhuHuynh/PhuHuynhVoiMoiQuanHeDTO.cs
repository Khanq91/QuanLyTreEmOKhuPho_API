namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class PhuHuynhVoiMoiQuanHeDTO
    {
        public int PhuHuynhID { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SDT { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string NgheNghiep { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string TonGiao { get; set; } = string.Empty;
        public string DanToc { get; set; } = string.Empty;
        public string QuocTich { get; set; } = string.Empty;
        public string Anh { get; set; } = string.Empty;

        // Danh sách mối quan hệ với trẻ em
        public List<MoiQuanHeVoiTreEmDTO> DanhSachMoiQuanHe { get; set; } = new();
    }
}
