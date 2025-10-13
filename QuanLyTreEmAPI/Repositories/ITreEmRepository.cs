using QuanLyTreEmAPI.DTOs.TreEm;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface ITreEmRepository
    {
        Task<(List<TreEm> items, int totalCount)> GetAllAsync(TreEmFilterDTO filter);
        Task<TreEm> GetByIdAsync(int id);
        Task<TreEm> CreateAsync(TreEm treEm);
        Task<bool> UpdateAsync(TreEm treEm);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<TreEm>> GetTreEmCanUuTienAsync(int top = 10);
        Task<List<TreEm>> GetTreEmNghiHocNhieuAsync();
        Task<List<TreEm>> GetTreEmChuaVaoLop1Async();
        Task<int> GetTongSoTreEmAsync();
        Task<List<object>> GetTreEmTheoKhuPhoAsync();
    }
}
