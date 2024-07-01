using Azure.Storage.Blobs;
using AzureTranslate.Models;
using AzureTranslate.Services;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// BlobServiceClient configuration
builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetValue<string>("Azure:BlobStorage:ConnectionString")));

// Controllers with views
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AzureTranslate API", Version = "v1" });
});
builder.Services.Configure<TranslationApiSettings>(builder.Configuration.GetSection("Azure:CognitiveServices:Translator"));

builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<ImageAnalysisService>();

// ComputerVisionClient configuration
builder.Services.AddSingleton<ComputerVisionClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var endpoint = config["Azure:CognitiveServices:Endpoint"];
    var key = config["Azure:CognitiveServices:Key"];
    return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
    {
        Endpoint = endpoint
    };
});

builder.Services.AddHttpClient("TranslationClient", httpClient =>
{
    var config = builder.Configuration;
    httpClient.BaseAddress = new Uri(config["Azure:CognitiveServices:Translator:TextEndpoint"]);
    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config["Azure:CognitiveServices:Translator:Key"]);
    var region = config["Azure:Translator:Region"];
    if (!string.IsNullOrEmpty(region))
    {
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);
    }
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
   
}
else
{
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();