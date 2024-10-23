using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Security.Cryptography;
namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly OnlineShopContext _context;

        public RoleService(OnlineShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetRoles(int page, int pageSize)
        {
            var query = _context.ChucVus.AsQueryable();

            var totalItemCount = await query.CountAsync();
            var roles = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new OkObjectResult(new
            {
                roles = roles,
                totalItems = totalItemCount,
                page = page,
                pageSize = pageSize
            });
        }

        public async Task<IActionResult> AddRole(ChucVuQuyen roleDto)
        {
            var role = new ChucVu
            {
                Ten = roleDto.TenCV
            };
            _context.ChucVus.Add(role);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }

        public async Task<IActionResult> UpdateRole(int roleId, ChucVuQuyen roleDto)
        {
            var role = await _context.ChucVus.FindAsync(roleId);
            if (role == null)
            {
                return new NotFoundObjectResult(new { status = false, message = "Không tìm thấy vai trò" });
            }

            role.Ten = roleDto.TenCV;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }

        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var role = await _context.ChucVus.FindAsync(roleId);
            if (role == null)
            {
                return new NotFoundObjectResult(new { status = false, message = "Không tìm thấy vai trò" });
            }

            _context.ChucVus.Remove(role);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }

        public async Task<IActionResult> GetPermissions(int roleId)
        {
            var permissions = await (from qcv in _context.CvQAs
                                     join q in _context.Quyens on qcv.MaQ equals q.MaQ
                                     join a in _context.ActionTs on qcv.MaA equals a.MaA
                                     where qcv.MaCv == roleId
                                     select new ChucVuQuyen
                                     {
                                         MaQ = q.MaQ,
                                         TenQ = q.Ten,
                                         MaA = a.MaA,
                                         TenA = a.TenA,
                                         ControllerName = q.ControllerName,
                                         ActionName = q.ActionName
                                     }).ToListAsync();

            return new OkObjectResult(new { permissions = permissions });
        }

        public async Task<IActionResult> UpdatePermissions(int roleId, ChucVuQuyen permissionUpdateDto)
        {
            // Xóa các quyền cũ
            var oldPermissions = _context.CvQAs.Where(q => q.MaCv == roleId);
            _context.CvQAs.RemoveRange(oldPermissions);

            // Thêm các quyền mới
            var newCVA = new CvQA
            {
                MaCv = roleId,
                MaQ = permissionUpdateDto.MaQ,
                MaA = permissionUpdateDto.MaA
            };
            _context.CvQAs.AddRange(newCVA);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }
    }
}
