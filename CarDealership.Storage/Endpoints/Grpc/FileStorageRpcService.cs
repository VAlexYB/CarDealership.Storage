using CarDealership.Storage.Services.Abstraction;
using Grpc.Core;
using Storage;

namespace CarDealership.Storage.Endpoints.Grpc
{
    public class FileStorageRpcService : FileStorage.FileStorageBase
    {
        private readonly IFileStorageService _fileStorageService;

        private const int BUFFER_SIZE = 64 * 1024;

        public FileStorageRpcService(IFileStorageService fileStorageService)
        {
           _fileStorageService = fileStorageService;
        }

        public override async Task<UploadResponse> UploadFile(IAsyncStreamReader<UploadRequest> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                using var ms = new MemoryStream(request.Content.ToByteArray());
                await _fileStorageService.UploadFileAsync(request.Directory, request.FileName, ms, "application/octet-stream");
            }
            return new UploadResponse { Message = "Файл загружен!" };
        }

        public override async Task DownloadFile(DownloadRequest request, IServerStreamWriter<DownloadResponse> responseStream, ServerCallContext context)
        {
            await using var stream = await _fileStorageService.DownloadFileAsync(request.Directory, request.FileName);
            
            var buffer = new byte[BUFFER_SIZE];

            int bytesRead = 0;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var response = new DownloadResponse
                {
                    Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead)
                };

                await responseStream.WriteAsync(response);
            }            
        }
    }
}
