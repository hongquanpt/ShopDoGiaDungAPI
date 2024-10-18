using Minio.DataModel.Args;
using Minio;


namespace ShopDoGiaDungAPI.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;  // Thay đổi kiểu thành IMinioClient
        private readonly string _bucketName;
        private readonly string _endpoint;

        public MinioService(IConfiguration configuration)
        {
            // Lấy thông tin từ appsettings.json
            _endpoint = configuration["MinIO:Endpoint"];
            var accessKey = configuration["MinIO:AccessKey"];
            var secretKey = configuration["MinIO:SecretKey"];
            _bucketName = configuration["MinIO:BucketName"];

            // Khởi tạo MinioClient với IMinioClient
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

            // Trả về URL của file đã upload
            return $"{_endpoint}/{_bucketName}/{fileName}";
        }

        public async Task DeleteFileAsync(string fileName)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucketName).WithObject(fileName));
        }
    }
}
