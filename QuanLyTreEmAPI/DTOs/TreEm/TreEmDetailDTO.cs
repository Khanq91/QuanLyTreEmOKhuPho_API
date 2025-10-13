namespace QuanLyTreEmAPI.DTOs.TreEm
{
    public class TreEmDetailDTO
    {
        public int TreEmID { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public int Tuoi { get; set; }
        public string GioiTinh { get; set; }
        public string TonGiao { get; set; }
        public string DanToc { get; set; }
        public string QuocTich { get; set; }
        public string TinhTrang { get; set; }
        public TruongHocDTO TruongHoc { get; set; }
        public KhuPhoDTO KhuPho { get; set; }
        public string MucUuTien { get; set; }
        public List<HoanCanhDTO> DanhSachHoanCanh { get; set; }
        public List<PhuHuynhDTO> DanhSachPhuHuynh { get; set; }
        public List<PhieuHocTapDTO> DanhSachPhieuHocTap { get; set; }
        public List<HoTroPhucLoiDTO> DanhSachHoTro { get; set; }
        public List<VanDongDTO> DanhSachVanDong { get; set; }
        public ThongKeDTO ThongKe { get; set; }
    }
}
