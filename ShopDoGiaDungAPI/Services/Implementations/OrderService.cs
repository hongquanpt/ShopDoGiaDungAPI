﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly OnlineShopContext _context;
        private readonly IMinioService _minioService;

        public OrderService(OnlineShopContext context, IMinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        public async Task<List<Donhang>> GetPendingOrdersAsync()
        {
            var orders = await _context.Donhangs
                .Where(order => order.TinhTrang == 1)
                .ToListAsync();
            return orders;
        }
        // Admin functions
        public IActionResult GetOrders(int? status, int page, int pageSize)
        {
            var allOrders = from a in _context.Donhangs
                            join b in _context.Vanchuyens on a.MaDonHang equals b.MaDonHang
                            select new MyOrder()
                            {
                                MaDonHang = a.MaDonHang,
                                TongTien = a.TongTien,
                                NguoiNhan = b.NguoiNhan,
                                DiaChi = b.DiaChi,
                                NgayMua = a.NgayLap,
                                TinhTrang = a.TinhTrang
                            };

            if (status.HasValue)
            {
                allOrders = allOrders.Where(o => o.TinhTrang == status.Value);
            }

            var model = allOrders.OrderByDescending(o => o.MaDonHang)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            var totalItemCount = allOrders.Count();

            return new OkObjectResult(new
            {
                data = model,
                totalItems = totalItemCount,
                page = page,
                pageSize = pageSize,
                totalPages = (totalItemCount + pageSize - 1) / pageSize
            });
        }

        public IActionResult ConfirmOrder(int orderId)
        {
            var dh = _context.Donhangs.Find(orderId);
            if (dh != null)
            {
                dh.TinhTrang = 2; // Xác nhận đơn hàng
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false });
            }
        }

        public IActionResult ShipOrder(int orderId)
        {
            var dh = _context.Donhangs.Find(orderId);
            if (dh != null)
            {
                dh.TinhTrang = 3; // Đã vận chuyển
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false });
            }
        }

        public IActionResult CancelOrder(int orderId)
        {
            var dh = _context.Donhangs.Find(orderId);
            if (dh != null)
            {
                dh.TinhTrang = 4; // Đã hủy đơn hàng
                _context.SaveChanges();
                return new OkObjectResult(new { status = true });
            }
            else
            {
                return new NotFoundObjectResult(new { status = false });
            }
        }

        public async Task<List<MyOrderDetail>> GetOrderDetails(int orderId)
        {
            var orderDetails = from a in _context.Chitietdonhangs
                               join b in _context.Sanphams on a.MaSp equals b.MaSp
                               where a.MaDonHang == orderId
                               select new MyOrderDetail()
                               {
                                   MaSanPham = b.MaSp,
                                   TenSP = b.TenSp,
                                   Anh = b.Anh1,
                                   GiaBan = b.GiaTien,
                                   SoLuong = a.SoLuongMua,
                                   ThanhTien = b.GiaTien * a.SoLuongMua
                               };

            var orderDetailsList = orderDetails.ToList();
            foreach (var sp in orderDetailsList)
            {
                sp.Anh = await _minioService.GetPreSignedUrlAsync(sp.Anh);
            }

            return orderDetailsList;
        }


        // Home functions
        public async Task<IActionResult> GetUserOrders(int userId, string typeMenu, int pageIndex, int pageSize)
        {
            IQueryable<Donhang> query = typeMenu switch
            {
                "tatca" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId),
                "chuathanhtoan" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId && a.TinhTrang == 0),
                "choxacnhan" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId && a.TinhTrang == 1),
                "dangvanchuyen" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId && a.TinhTrang == 2),
                "dahoanthanh" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId && a.TinhTrang == 3),
                "dahuy" => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId && a.TinhTrang == 4),
                _ => _context.Donhangs.Include(item => item.Vanchuyen).Where(a => a.MaTaiKhoan == userId)
            };

            var count = await query.CountAsync();
            var dt = await query.OrderByDescending(item => item.MaDonHang).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new OkObjectResult(new
            {
                donhang = dt,
                typeMenu = typeMenu,
                totalCount = count
            });
        }

        public async Task<IActionResult> CancelUserOrder(int orderId, int userId)
        {
            var dh = await _context.Donhangs.FirstOrDefaultAsync(d => d.MaDonHang == orderId && d.MaTaiKhoan == userId);
            if (dh == null)
            {
                return new NotFoundResult();
            }

            dh.TinhTrang = 4;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }

        public async Task<IActionResult> ConfirmOrderReceived(int orderId, int userId)
        {
            var dh = await _context.Donhangs.FirstOrDefaultAsync(d => d.MaDonHang == orderId && d.MaTaiKhoan == userId);
            if (dh == null)
            {
                return new NotFoundResult();
            }

            dh.TinhTrang = 3;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { status = true });
        }
    }
}