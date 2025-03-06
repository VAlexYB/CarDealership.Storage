namespace CarDealership.Storage.Services.Abstraction
{
    public interface IFileStorageService
    {
        Task UploadFileAsync(string directory, string fileName, Stream content, string contentType);
        Task<Stream> DownloadFileAsync(string directory, string fileName);
    }
}
