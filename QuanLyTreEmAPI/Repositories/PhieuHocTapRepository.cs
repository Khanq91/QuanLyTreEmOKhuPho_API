using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Repositories
{
    public class PhieuHocTapRepository : IPhieuHocTapRepository
    {
        private readonly QuanLyTreEmAPI.Data.QuanLyTreEmContext _context;

        public PhieuHocTapRepository(QuanLyTreEmAPI.Data.QuanLyTreEmContext context)
        {
            _context = context;
        }

        public async Task<PhieuHocTap> CreateAsync(PhieuHocTap phieuHocTap)
        {
            //phieuHocTap.NgayCapNhat = DateTime.Now;
            phieuHocTap.NgayCapNhat = DateOnly.FromDateTime(DateTime.Now);
            _context.PhieuHocTaps.Add(phieuHocTap);
            await _context.SaveChangesAsync();
            return phieuHocTap;
        }

        public async Task<List<PhieuHocTap>> GetByTreEmIdAsync(int treEmId)
        {
            return await _context.PhieuHocTaps
                .Include(p => p.Truong)
                .Include(p => p.Lop)
                .Where(p => p.TreEmId == treEmId)
                .OrderByDescending(p => p.NgayCapNhat)
                .ToListAsync();
        }
    }
}
