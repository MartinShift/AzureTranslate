using Azure.Storage.Blobs;

namespace AzureTranslate.Services;

public class BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration)
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly string _containerName = configuration.GetValue<string>("Azure:BlobStorage:ContainerName");


    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(file.FileName);
        await blobClient.UploadAsync(file.OpenReadStream(), true);
        return blobClient.Uri.ToString();
    }
}