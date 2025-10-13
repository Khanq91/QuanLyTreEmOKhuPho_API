namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class CreateTreEmDTO
    {
        // Bước 1: Thông tin cơ bản
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
        public string TinhTrang { get; set; }
        public int? KhuPhoID { get; set; }
        public List<int> HoanCanhIDs { get; set; }

        // Bước 2: Thông tin phụ huynh
        public List<CreatePhuHuynhDTO> DanhSachPhuHuynh { get; set; }

        // Bước 3: Thông tin học tập
        public int? TruongID { get; set; }
        public int? LopID { get; set; }
        public PhieuHocTapDTO PhieuHocTapDauTien { get; set; }
    }
}
