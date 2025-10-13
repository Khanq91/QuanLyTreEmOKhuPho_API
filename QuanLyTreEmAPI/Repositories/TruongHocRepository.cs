using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class TruongHocRepository : ITruongHocRepository
    {
        private readonly QuanLyTreEmAPI.Data.QuanLyTreEmContext _context;

        public TruongHocRepository(QuanLyTreEmAPI.Data.QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<List<TruongHoc>> GetAllAsync()
        {
            return await _context.TruongHocs.ToListAsync();
        }

        public async Task<TruongHoc> GetByIdAsync(int id)
        {
            return await _context.TruongHocs
                .Include(t => t.LopHocs)
                .FirstOrDefaultAsync(t => t.TruongId == id);
        }

        public async Task<List<LopHoc>> GetLopHocByTruongAsync(int truongId)
        {
            return await _context.LopHocs
                .Where(l => l.TruongId == truongId)
                .ToListAsync();
        }
    }
}
