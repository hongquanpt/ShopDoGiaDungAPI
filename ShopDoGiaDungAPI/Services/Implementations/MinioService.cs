using Minio.DataModel.Args;
using Minio;
using ShopDoGiaDungAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly string _endpoint;

        public MinioService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            // Kiểm tra nếu ứng dụng chạy trên localhost hay không
            var isLocalhost = httpContextAccessor.HttpContext.Request.Host.Host == "localhost" ||
                              httpContextAccessor.HttpContext.Request.Host.Host == "127.0.0.1";

            // Quyết định endpoint dựa trên kiểm tra localhost
            _endpoint = isLocalhost ? configuration["MinIO:Endpoint"] : "192.168.1.40:9000";  // Nếu không phải localhost thì dùng 192.168.1.40

            var accessKey = configuration["MinIO:AccessKey"];
            var secretKey = configuration["MinIO:SecretKey"];
            _bucketName = configuration["MinIO:BucketName"];

            _minioClient = new MinioClient()
                .WithEndpoint(_endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();
        }


        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // Tạo tên file duy nhất
            var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);

            // Kiểm tra bucket đã tồn tại hay chưa, nếu chưa thì tạo mới
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
            }

            using (var stream = file.OpenReadStream())
            {
                // Upload file lên MinIO
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType));
            }

            // Trả về tên tệp
            return fileName;
        }

        public async Task<string> GetPreSignedUrlAsync(string fileName, HttpContext httpContext)
        {
            // Kiểm tra nếu ứng dụng chạy trên localhost hay không
            var isLocalhost = httpContext.Request.Host.Host == "localhost" || httpContext.Request.Host.Host == "127.0.0.1";

            // Quyết định dùng localhost hay IP dựa trên kiểm tra
            var host = isLocalhost ? "localhost" : "192.168.1.40";  // Chọn địa chỉ host phù hợp
            var protocol = "http";  
            var port = "9000";  // Cổng MinIO

            // Tạo URL Presigned từ MinIO
            string presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithExpiry(3600)); // URL có hiệu lực trong 3600 giây (1 giờ)

            // Tạo lại URL có cổng và host phù hợp
            Uri minioUri = new Uri(presignedUrl);
            string finalUrl = $"{protocol}://{host}:{port}{minioUri.AbsolutePath}{minioUri.Query}";

            return finalUrl;
        }


        public async Task DeleteFileAsync(string fileName)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucketName).WithObject(fileName));
        }
    }
}
