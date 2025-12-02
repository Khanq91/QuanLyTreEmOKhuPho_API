using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyTreEmAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTreEmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class KhuPhoController : ControllerBase
    {
        private readonly QuanLyTreEmContext _context;

        public KhuPhoController(QuanLyTreEmContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var khuPhos = await _context.KhuPhos.ToListAsync();
            return Ok(khuPhos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var khuPho = await _context.KhuPhos.FindAsync(id);
            if (khuPho == null)
                return NotFound();

            return Ok(khuPho);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] KhuPho khuPho)
        {
            _context.KhuPhos.Add(khuPho);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = khuPho.KhuPhoId }, khuPho);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] KhuPho khuPho)
        {
            var existing = await _context.KhuPhos.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.TenKhuPho = khuPho.TenKhuPho;
            existing.DiaChi = khuPho.DiaChi;
            existing.QuanHuyen = khuPho.QuanHuyen;
            existing.ThanhPho = khuPho.ThanhPho;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var khuPho = await _context.KhuPhos.FindAsync(id);
            if (khuPho == null)
                return NotFound();

            _context.KhuPhos.Remove(khuPho);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }
        [HttpGet("SapToi")]
        public async Task<ActionResult<List<SuKien>>> LaySuKienSapToi([FromQuery] int? KhuPhoID)
        {
            try
            {
                var query = _context.SuKiens
                    .Where(sk => sk.NgayBatDau.HasValue
                         && sk.NgayBatDau.Value > DateOnly.FromDateTime(DateTime.Now))
                    .OrderBy(sk => sk.NgayBatDau);
                if (KhuPhoID.HasValue)
                {
                    query = (IOrderedQueryable<SuKien>)query.Where(sk => sk.KhuPhoId == KhuPhoID.Value);
                }

                var suKiens = await query.ToListAsync();
                return Ok(suKiens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách sự kiện", error = ex.Message });

            }
        }
    }
}