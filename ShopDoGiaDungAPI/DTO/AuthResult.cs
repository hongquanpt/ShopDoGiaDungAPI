namespace ShopDoGiaDungAPI.DTO
{
    public class AuthResult
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public string Message { get; set; }
    }
}
