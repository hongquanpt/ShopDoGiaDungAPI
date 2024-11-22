using System.Security;

namespace ShopDoGiaDungAPI.DTO
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int MaDonVi { get; set; }
        public string TenDonVi { get; set; }
        public ICollection<PermissionDto> Permissions { get; set; }
        public List<string> Roles { get; set; }
    }
}
