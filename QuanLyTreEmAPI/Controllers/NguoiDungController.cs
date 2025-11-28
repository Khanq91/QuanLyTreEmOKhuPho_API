using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using QuanLyTreEmAPI.Models;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;

        public NguoiDungController(QuanLyTreEmContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NguoiDung>>> GetAll()
        {
            var NguoiDung= await _context.NguoiDungs.ToListAsync();
            return Ok(NguoiDung);
        }
        [HttpGet("Login")]
        public async Task<IActionResult> Login(string SDT, string MatKhau)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.SDT == SDT && u.MatKhau == MatKhau && (u.VaiTro== "Cán bộ" || u.VaiTro== "Admin"));

            if (user == null)
                return Unauthorized(); 
            return Ok(new
            {
                UserId = user.UserId,
                Ten = user.HoTen,
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai
            });
        }


    }
}
