namespace AzureTranslate.Models;
public class TranslationApiSettings
{
    public string TextEndpoint { get; set; }
    public string DocumentEndpoint { get; set; }
    public string Key { get; set; }
    public string Region { get; set; }
    public string DefaultLanguage { get; set; }
}