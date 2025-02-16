# Demo

This is a simple demo solution composed of 3 services to run.

No best practice is followed here. Primary purpose is to serve as support for demonstrating how to run this kind of solution in Kubernetes with auto generated manifests tailored for local debugging.

## How it works

### Setup

- SQL Server is required locally, with a "Demo" database.
- Azurite is required locally, with a blob container named "demo".
- Azure Service Bus Emulator is required (or actual Azure Service Bus with appropriate connection string). A service bus queue named "queue.1" will be used by the demo.

**Demo.WeatherForecastApi**: set user secrets (right click, "Manage User Secrets"):

```json
{

  "SqlDatabase": {
    "ConnectionString": "Data Source=localhost;Initial Catalog=Demo;Trusted_Connection=True;TrustServerCertificate=true;"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
  }
}
```

**Demo.Frontend** set user secrets (right click, "Manage User Secrets"):

```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
  }
}
```

**Demo.BackendService** set user secrets (right click, "Manage User Secrets"):

```json
{
  "BlobStorage": {
    "ConnectionString": "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1/;"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
  }
}
```

### Run

Start with **Demo.WeatherForecastApi** and go to http://localhost:5110/swagger to try out the API.

Then run **Demo.BackendService** and **Demo.Frontend**. Go to frontend http://localhost:5255 and click on "Trigger" button in the web page.

This should trigger an event sent to Service Bus queue, consumed by **Demo.BackendService** which will then call **Demo.WeatherForecastApi** through HTTP.

There is no feedback in the web UI. Instead, a blob will be uploaded by **Demo.BackendService** with result from **Demo.WeatherForecastApi**.

