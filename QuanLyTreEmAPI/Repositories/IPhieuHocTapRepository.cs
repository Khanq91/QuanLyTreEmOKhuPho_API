using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Repositories
{
    public interface IPhieuHocTapRepository
    {
        Task<PhieuHocTap> CreateAsync(PhieuHocTap phieuHocTap);
        Task<List<PhieuHocTap>> GetByTreEmIdAsync(int treEmId);
    }
}
