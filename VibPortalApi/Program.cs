using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using IBM.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VibPortalApi;
using VibPortalApi.Data;
using VibPortalApi.Models.Settings;
using VibPortalApi.Services.B2B;
using VibPortalApi.Services.B2B.Extractors;
using VibPortalApi.Services.Euravib;
using VibPortalApi.Services.Gmail;
using VibPortalApi.Services.Zenya;
using VibPortalApi.Services.B2B.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GmailSettings>(
    builder.Configuration.GetSection("GmailSettings"));
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CorsPolicy",
                      policy =>
                      {
                          policy.SetIsOriginAllowed(origin => true);
                          policy.AllowAnyMethod();
                          policy.AllowAnyHeader();
                          policy.AllowCredentials();
                      });
});



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes(); // Fix for nullable issues
});

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLserver")));

// Register the DbContext with the IBM DB2 provider
builder.Services.AddDbContext<DB2Context>(options =>
    options.UseDb2(builder.Configuration.GetConnectionString("DB2Connection"), db2Options =>
    {
        // Optionally, configure provider-specific settings here.
        // For example: db2Options.SetServerInfo(IBM.EntityFrameworkCore.Db2.Db2ServerType.LUW);
    }));


builder.Services.AddScoped<IManageMsdsService, ManageMsdsService>();
builder.Services.AddScoped<PdfExtractor_Akzo>();
builder.Services.AddScoped<PdfExtractor_Basf>();
builder.Services.AddScoped<PdfExtractor_Beckers>();
builder.Services.AddScoped<PdfExtractor_Brillux>();
builder.Services.AddScoped<PdfExtractor_Kluthe>();
builder.Services.AddScoped<PdfExtractor_Monopol>();
builder.Services.AddScoped<PdfExtractor_Ppg>();
builder.Services.AddScoped<PdfExtractor_Valspar>();
builder.Services.AddScoped<IPdfExtractorFactory, PdfExtractorFactory>();
builder.Services.AddScoped<IVibImportService, VibImportService>();
builder.Services.AddScoped<IEuravibService, EuravibService>();
builder.Services.AddScoped<IGmailService, GmailService>();
builder.Services.AddScoped<IB2bPdfExtractorFactory, B2bPdfExtractorFactory>();
builder.Services.AddScoped<B2bPdfExtractor_Aludium>();

builder.Services.AddScoped<IB2BFormRecognizerFactory, B2BFormRecognizerFactory>();
builder.Services.AddScoped<FormRecognizerAludiumMapper>();
builder.Services.AddScoped<FormRecognizerBeckersMapper>();
builder.Services.AddScoped<IB2BImportOc, B2BImportOc>();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureFormRecognizer:Endpoint"];
    var key = config["AzureFormRecognizer:ApiKey"];
    return new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
});


//builder.Services.AddHttpClient<IZenyaService, ZenyaService>();
HttpClientHandler insecureHandler = new HttpClientHandler();
insecureHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

builder.Services.AddHttpClient<IZenyaService, ZenyaService>()
    .ConfigurePrimaryHttpMessageHandler(() => insecureHandler);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
