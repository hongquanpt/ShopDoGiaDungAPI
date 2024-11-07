using Minio.DataModel.Args;
using Minio;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly string _endpoint;

        public MinioService(IConfiguration configuration)
        {
            _endpoint = configuration["MinIO:Endpoint"];
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

            // Không tạo Presigned URL ở đây
            // Chỉ trả về tên tệp
            return fileName;
        }

        public async Task<string> GetPreSignedUrlAsync(string fileName)
        {
            try
            {
                // Tạo Pre-signed URL có thời hạn (ví dụ: 1 giờ)
                string presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithExpiry(3600)); // URL có hiệu lực trong 3600 giây (1 giờ)

                return presignedUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo Pre-signed URL: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucketName).WithObject(fileName));
        }
    }
}
