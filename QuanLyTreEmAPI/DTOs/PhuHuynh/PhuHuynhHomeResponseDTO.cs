namespace QuanLyTreEmAPI.DTOs.PhuHuynh
{
    public class PhuHuynhHomeResponseDTO
    {
        public List<TreEmDropdownDTO> DanhSachCon { get; set; }
        public int TreEmMacDinhId { get; set; }
        public ThongTinHocTapDTO ThongTinHocTap { get; set; }
        public List<HoTroInfoDTO> HoTroDaNhan { get; set; }
        public List<SuKienInfoDTO> SuKienSapToi { get; set; }
        public List<ThongBaoInfoDTO> ThongBaoChuaDoc { get; set; }
        public int SoThongBaoChuaDoc { get; set; }
    }
}
