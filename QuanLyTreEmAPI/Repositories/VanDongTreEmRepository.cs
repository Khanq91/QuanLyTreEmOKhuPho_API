using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class VanDongTreEmRepository : IVanDongTreEmRepository
    {
        private readonly QuanLyTreEmAPI.Data.QuanLyTreEmContext _context;

        public VanDongTreEmRepository(QuanLyTreEmAPI.Data.QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<VanDongTreEm> CreateAsync(VanDongTreEm vanDong)
        {
            //vanDong.NgayVanDong = DateTime.Now;
            vanDong.NgayVanDong = DateOnly.FromDateTime(DateTime.Now);
            _context.VanDongTreEms.Add(vanDong);
            await _context.SaveChangesAsync();
            return vanDong;
        }

        public async Task<List<VanDongTreEm>> GetByTreEmIdAsync(int treEmId)
        {
            return await _context.VanDongTreEms
                .Include(v => v.HoanCanh)
                .Include(v => v.NguoiDung)
                .Where(v => v.TreEmId == treEmId)
                .OrderByDescending(v => v.NgayVanDong)
                .ToListAsync();
        }
    }
}
