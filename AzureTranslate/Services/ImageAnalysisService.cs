using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Text;

namespace AzureTranslate.Services;

public class ImageAnalysisService
{
    private ComputerVisionClient _computerVisionClient;
    private readonly IConfiguration _configuration;

    public ImageAnalysisService(IConfiguration configuration)
    {
        _configuration = configuration;
        _computerVisionClient = Authenticate(_configuration.GetValue<string>("Azure:CognitiveServices:ComputerVision:Key"), _configuration.GetValue<string>("Azure:CognitiveServices:ComputerVision:Endpoint"));
    }

    public async Task<ImageAnalysis> AnalyzeImageAsync(string imageUrl)
    {
        _computerVisionClient = Authenticate(_configuration.GetValue<string>("Azure:CognitiveServices:ComputerVision:Key"), _configuration.GetValue<string>("Azure:CognitiveServices:ComputerVision:Endpoint"));
        var features = new List<VisualFeatureTypes?> { VisualFeatureTypes.Description, VisualFeatureTypes.Adult, VisualFeatureTypes.Faces, VisualFeatureTypes.Categories, VisualFeatureTypes.Tags, VisualFeatureTypes.Objects };
        var analysisResult = await _computerVisionClient.AnalyzeImageAsync(imageUrl, features);
        return analysisResult;
    }

    public async Task<string> ReadTextFromImageAsync(string imageUrl)
    {
        var textHeaders = await _computerVisionClient.ReadAsync(imageUrl);
        string operationLocation = textHeaders.OperationLocation;

        string operationId = operationLocation.Substring(operationLocation.LastIndexOf('/') + 1);

        ReadOperationResult results;
        do
        {
            results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
        }
        while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

        var textResult = new StringBuilder();
        if (results.Status == OperationStatusCodes.Succeeded)
        {
            foreach (var page in results.AnalyzeResult.ReadResults)
            {
                foreach (var line in page.Lines)
                {
                    textResult.AppendLine(line.Text);
                }
            }
        }
        return textResult.ToString();
    }

    public static ComputerVisionClient Authenticate(string key, string endpoint)
    {
        ComputerVisionClient visionClient = new (
        new ApiKeyServiceClientCredentials(key))
        { Endpoint = endpoint };
        return visionClient;
    }
}
