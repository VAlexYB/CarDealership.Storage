using CarDealership.Storage.Services.Abstraction;
using CarDealership.Storage.Settings;
using Microsoft.Extensions.Options;
using Minio.DataModel.Args;
using Minio;

namespace CarDealership.Storage.Services.Implementation
{
    public class MinioFileStorageService : IFileStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly Lazy<Task> _ensureBucketExistsTask;

        private const string DefaultBucket = "storage";

        public MinioFileStorageService(IMinioClient minioClient)
        {
            _minioClient = minioClient;

            _ensureBucketExistsTask = new Lazy<Task>(() => EnsureBucketExistsAsync(DefaultBucket));
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);

            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }
        }  

        public async Task<Stream> DownloadFileAsync(string directory, string fileName)
        {
            string objectName = $"{directory.TrimEnd('/')}/{fileName}";

            var ms = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(DefaultBucket)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms));

            await _minioClient.GetObjectAsync(getObjectArgs);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;            
        }

        public async Task UploadFileAsync(string directory, string fileName, Stream content, string contentType)
        {
            await _ensureBucketExistsTask.Value; // однократная проверка, что корневой бакет существует

            string objectName = $"{directory.TrimEnd('/')}/{fileName}";

            await EnsureBucketExistsAsync(DefaultBucket);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(DefaultBucket)
                .WithObject(objectName)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
        }
    }
}
