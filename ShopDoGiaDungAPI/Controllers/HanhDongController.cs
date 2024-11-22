using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HanhDongController : ControllerBase
    {
        private readonly OnlineShopContext _context;

        public HanhDongController(OnlineShopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả các Hành Động
        /// </summary>
        /// <returns>Danh sách HanhDongDto</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HanhDong>>> GetAllHanhDong()
        {
            var hanhDongs = await _context.HanhDongs
                .Select(hd => new HanhDong
                {
                    MaHanhDong = hd.MaHanhDong,
                    TenHanhDong = hd.TenHanhDong
                })
                .ToListAsync();

            return Ok(hanhDongs);
        }

        /// <summary>
        /// Lấy thông tin một Hành Động theo MaHanhDong
        /// </summary>
        /// <param name="id">MaHanhDong</param>
        /// <returns>HanhDongDto</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<HanhDong>> GetHanhDong(int id)
        {
            var hanhDong = await _context.HanhDongs
                .Where(hd => hd.MaHanhDong == id)
                .Select(hd => new HanhDong
                {
                    MaHanhDong = hd.MaHanhDong,
                    TenHanhDong = hd.TenHanhDong
                })
                .FirstOrDefaultAsync();

            if (hanhDong == null)
            {
                return NotFound();
            }

            return Ok(hanhDong);
        }

        /// <summary>
        /// Thêm mới một Hành Động
        /// </summary>
        /// <param name="hanhDongDto">Thông tin HanhDongDto</param>
        /// <returns>HanhDongDto vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<HanhDong>> CreateHanhDong([FromBody] HanhDong hanhDongDto)
        {
            if (hanhDongDto == null)
            {
                return BadRequest();
            }

            var hanhDong = new HanhDong
            {
                TenHanhDong = hanhDongDto.TenHanhDong
            };

            _context.HanhDongs.Add(hanhDong);
            await _context.SaveChangesAsync();

            hanhDongDto.MaHanhDong = hanhDong.MaHanhDong;

            return CreatedAtAction(nameof(GetHanhDong), new { id = hanhDong.MaHanhDong }, hanhDongDto);
        }

        /// <summary>
        /// Cập nhật một Hành Động
        /// </summary>
        /// <param name="id">MaHanhDong</param>
        /// <param name="hanhDongDto">Thông tin HanhDongDto cập nhật</param>
        /// <returns>Không có nội dung</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHanhDong(int id, [FromBody] HanhDong hanhDongDto)
        {
            if (id != hanhDongDto.MaHanhDong)
            {
                return BadRequest("MaHanhDong không khớp với ID trong URL.");
            }

            var hanhDong = await _context.HanhDongs.FindAsync(id);
            if (hanhDong == null)
            {
                return NotFound();
            }

            hanhDong.TenHanhDong = hanhDongDto.TenHanhDong;

            _context.Entry(hanhDong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HanhDongs.Any(hd => hd.MaHanhDong == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Xóa một Hành Động
        /// </summary>
        /// <param name="id">MaHanhDong</param>
        /// <returns>Không có nội dung</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHanhDong(int id)
        {
            var hanhDong = await _context.HanhDongs.FindAsync(id);
            if (hanhDong == null)
            {
                return NotFound();
            }

            _context.HanhDongs.Remove(hanhDong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    }
