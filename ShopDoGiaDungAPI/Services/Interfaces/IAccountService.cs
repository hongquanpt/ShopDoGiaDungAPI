using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IAccountService
    {
        IActionResult GetAccounts( int page, int pageSize);
        IActionResult UpdateAccountRole(int matk, int macv);
        IActionResult DeleteAccount(int matk);
        Task<Taikhoan> GetAccountByIdAsync(int maTaiKhoan);
    }
}
