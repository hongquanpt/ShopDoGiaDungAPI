namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<string> GetPreSignedUrlAsync(string fileName, HttpContext httpContext);
        Task DeleteFileAsync(string fileName);
    }
}
