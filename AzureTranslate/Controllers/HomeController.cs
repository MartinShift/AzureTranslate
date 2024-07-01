using AzureTranslate.Models;
using AzureTranslate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AzureTranslate.Controllers;


public class HomeController(BlobService blobService, ImageAnalysisService imageAnalysisService, IHttpClientFactory clientFactory, IOptionsSnapshot<TranslationApiSettings> apiSettings) : Controller
{
    private readonly BlobService _blobService = blobService;
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly TranslationApiSettings _apiSettings = apiSettings.Value;
    private readonly ImageAnalysisService _imageAnalysisService = imageAnalysisService;

    [HttpPost("translateText")]
    public async Task<IActionResult> TranslateText([FromForm] TranslationRequest translationRequest)
    {
        if (translationRequest == null || string.IsNullOrWhiteSpace(translationRequest.Text))
            return BadRequest("Text to translate is required.");

        string targetLanguage = string.IsNullOrWhiteSpace(translationRequest.TargetLanguage) ? _apiSettings.DefaultLanguage : translationRequest.TargetLanguage;

        var requestBody = new object[] { new { Text = translationRequest.Text } };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiSettings.Key);
        if (!string.IsNullOrEmpty(_apiSettings.Region))
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _apiSettings.Region);
        }
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var url = $"{_apiSettings.TextEndpoint}translate?api-version=3.0&to={targetLanguage}";
        var response = await client.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return Ok(jsonResponse);
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Failed to translate text");
        }
    }

    [HttpPost("translateImage")]
    public async Task<IActionResult> TranslateImage(TranslateImageRequest request)
    {
        if (request.Image == null || request.Image.Length == 0)
            return BadRequest("An image file is required.");

        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
            request.TargetLanguage = _apiSettings.DefaultLanguage;

        var imageUrl = await _blobService.UploadImageAsync(request.Image);

        var extractedText = await _imageAnalysisService.ReadTextFromImageAsync(imageUrl);

        if (string.IsNullOrWhiteSpace(extractedText))
            return BadRequest("No text found in the image to translate.");

        var translationRequest = new TranslationRequest { Text = extractedText, TargetLanguage = request.TargetLanguage };

        return await TranslateText(translationRequest);
    }

    public IActionResult Index()
    {
        return View();
    }
}
