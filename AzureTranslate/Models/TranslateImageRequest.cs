namespace AzureTranslate.Models;

public class TranslateImageRequest
{
    public IFormFile Image { get; set; }
    public string TargetLanguage { get; set; }
}
