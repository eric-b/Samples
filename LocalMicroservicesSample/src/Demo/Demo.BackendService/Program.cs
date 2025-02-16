using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Demo.BackendService;
using Demo.Infrastructure.OpenTelemetry.Extensions;
using Demo.WeatherForecastApi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddOpenTelemetry();

builder.Services.AddLogging();
builder.Services.AddSingleton(s => new ServiceBusClient(builder.Configuration.GetValue<string>("ServiceBus:ConnectionString")));
builder.Services.AddSingleton(s =>
{
    var blobService = new BlobServiceClient(builder.Configuration.GetValue<string>("BlobStorage:ConnectionString"));

    var containerName = builder.Configuration.GetValue<string>("BlobStorage:Container");
	try
	{
		blobService.CreateBlobContainer(containerName);
    }
	catch (RequestFailedException ex)
	{
        // HTTP 409 if container already exists
        if (ex.Status != 409)
            throw;
    }
    return blobService;
});

builder.Services.AddHostedService<DemoHostedService>();
builder.Services.Configure<DemoHostedServiceOptions>(builder.Configuration.GetSection("ServiceBus"));
builder.Services.Configure<DemoHostedServiceOptions>(builder.Configuration.GetSection("BlobStorage"));
builder.Services.AddSingleton<IValidateOptions<DemoHostedServiceOptions>, ValidateDemoHostedServiceOptions>();

builder.Services.AddHttpClient().AddHttpClient<WeatherForecastClient>(configure =>
{
    configure.BaseAddress = new Uri(builder.Configuration.GetValue<string>("HttpClient:WeatherForecastApi:BaseAddress") ?? "Missing app setting");
});

IHost host = builder.Build();
host.Run();