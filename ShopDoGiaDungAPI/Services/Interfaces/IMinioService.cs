namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<string> GetPreSignedUrlAsync(string fileName);
        Task DeleteFileAsync(string fileName);
    }
}
