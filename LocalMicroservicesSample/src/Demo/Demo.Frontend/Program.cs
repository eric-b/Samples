using Azure.Messaging.ServiceBus;
using Demo.Frontend;
using Demo.Infrastructure.OpenTelemetry.Extensions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.AddOpenTelemetry();

builder.Services.AddSingleton<Trigger>();
builder.Services.Configure<TriggerOptions>(builder.Configuration.GetSection("ServiceBus"));
builder.Services.AddSingleton<IValidateOptions<TriggerOptions>, ValidateTriggerOptions>();
builder.Services.AddSingleton(s =>
{
    return new ServiceBusClient(builder.Configuration.GetValue<string>("ServiceBus:ConnectionString"));
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
