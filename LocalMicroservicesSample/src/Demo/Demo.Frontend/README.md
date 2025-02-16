*Demo.Frontend* depends on:

- Azure Service Bus

Main page (index) contains a "Trigger" button that send a message on a queue.

This message is consumed by *Demo.BackendService* that will call *Demo.WeatherForecastApi* and store result in a blob storage.
