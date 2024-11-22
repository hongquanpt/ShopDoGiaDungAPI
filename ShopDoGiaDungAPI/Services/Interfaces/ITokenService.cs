using ShopDoGiaDungAPI.DTO;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserDto user);
    }
}
