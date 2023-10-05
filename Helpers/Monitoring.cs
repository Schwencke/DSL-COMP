using Serilog;
using Serilog.Enrichers.Span;

namespace Helpers
{
    public static class Monitoring
    {
        //public static readonly ActivitySource ActivitySource = new("DSL_COMP", "1.0.0");
        //private static TracerProvider _tracerProvider;

        static Monitoring()
        {
            /*// Configure tracing
            var serviceName = Assembly.GetCallingAssembly().GetName().Name;
            var version = "1.0.0";

            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddZipkinExporter((options) => options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans"))
                .AddConsoleExporter()
                .AddSource(ActivitySource.Name)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: version))
                .Build();*/

            // Configure logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithSpan()
                .WriteTo.Seq("http://seq:5341")
                .WriteTo.Console()
                .CreateLogger();

        }
    }
}
