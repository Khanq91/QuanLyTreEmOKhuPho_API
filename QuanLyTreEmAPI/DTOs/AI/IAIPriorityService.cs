namespace QuanLyTreEmAPI.DTOs.AI
{
    public interface IAIPriorityService
    {
        Task<List<PhanLoaiUuTienResponse>> PhanLoaiTatCa();
        Task<Top5Response> LayTop5UuTien();
        Task<PhanLoaiUuTienResponse> PhanLoaiMotTreEm(int treEmId);
        Task<List<PhanLoaiUuTienResponse>> PhanLoaiTheoMucDo(string mucDo);
    }
}
