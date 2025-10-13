using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface ITruongHocRepository
    {
        Task<List<TruongHoc>> GetAllAsync();
        Task<TruongHoc> GetByIdAsync(int id);
        Task<List<LopHoc>> GetLopHocByTruongAsync(int truongId);
    }
}
