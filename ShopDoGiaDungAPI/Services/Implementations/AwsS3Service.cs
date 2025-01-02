using Amazon.S3;
using Amazon.S3.Model;
using Minio.DataModel;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class AwsS3Service : IMinioService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsS3Service(IConfiguration configuration)
        {
            var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
            var awsRegion = configuration["AWS:Region"];

            _bucketName = configuration["AWS:BucketName"]; // Gán bucketName vào _bucketName

            // Tạo Amazon S3 Client
            _s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.GetBySystemName(awsRegion));
        }



        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                // Upload file lên S3
                await _s3Client.PutObjectAsync(putRequest);
            }

            return fileName;
        }


        public async Task<string> GetPreSignedUrlAsync(string fileName)
        {
            try
            {
                // Tạo yêu cầu Pre-signed URL
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    Expires = DateTime.UtcNow.AddHours(1) // URL có hiệu lực 1 giờ
                };

                // Tạo Pre-signed URL
                string url = _s3Client.GetPreSignedURL(request);
                return url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating pre-signed URL: {ex.Message}");
                throw;
            }
        }


        public async Task DeleteFileAsync(string fileName)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}
