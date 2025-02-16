using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Demo.WeatherForecastApi.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.BackendService
{
    public class DemoHostedService(IOptions<DemoHostedServiceOptions> options,
                                   ServiceBusClient serviceBus,
                                   BlobServiceClient blobService,
                                   WeatherForecastClient weatherForecastClient,
                                   ILogger<DemoHostedService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting hosted service");
            await using var processor = serviceBus.CreateProcessor(options.Value.QueueName);
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorMessageHandler;
            await processor.StartProcessingAsync(stoppingToken);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(10000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // .
            }

            logger.LogInformation("Hosted service terminated");
        }

        private Task ErrorMessageHandler(ProcessErrorEventArgs args)
        {
            logger.LogError(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var cancellationToken = args.CancellationToken;
            string body = args.Message.Body.ToString();
            await args.CompleteMessageAsync(args.Message);

            string result;
            try
            {
                result = await weatherForecastClient.GetWeather(cancellationToken);
                logger.LogInformation("{Message} - {WeatherForecast}", body, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to call weather forecast API.");
                return;
            }

            var container = blobService.GetBlobContainerClient(options.Value.Container);
            try
            {
                await container.UploadBlobAsync($"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-{Guid.NewGuid()}.json", BinaryData.FromString(result), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to call weather forecast API.");
            }
        }
    }
}
