namespace QuanLyTreEmAPI.DTOs.ManhThuongQuan
{
    public class ThongTinUngHo
    {
        public int? ManhThuongQuanId { get; set; }
        public decimal? SoTien { get; set; }
        public int SoLuongVatPham { get; set; }
        public string NgayUngHo { get; set; } // Nhận string từ client: "yyyy-MM-dd"
        public string? DoiTuong { get; set; }
        public string? TenVatPham { get; set; }
        public string? LoaiUngHo { get; set; }
        public string? GhiChu { get; set; }
        public List<FileUploadDto> Files { get; set; } // Nhận danh sách file Base64
                                                       //public PhieuMinhChungDto? PhieuMinhChungDto { get; set; }
    }

    public class FileUploadDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string FileData { get; set; } // Base64 string
        public string LoaiMinhChung { get; set; }
    }

    //public class PhieuMinhChungDto
    //{
    //    public string? LoaiMinhChung { get; set; }
    //    public string? MoTa { get; set; }
    //}
}
