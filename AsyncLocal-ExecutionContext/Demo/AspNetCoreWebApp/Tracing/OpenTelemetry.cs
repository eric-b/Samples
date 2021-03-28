using System.Diagnostics;

namespace AspNetCoreWebApp.Tracing
{
    static class OpenTelemetry
    {
        public static readonly ActivitySource ActivitySource = new ActivitySource("AspNetCoreWebApp");
    }
}
