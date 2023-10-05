using Calc_Service_API.Utils;
using Calc_Service_Sub.Utils;
using Events;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var serviceName = Assembly.GetCallingAssembly().GetName().Name;
var serviceVersion = "1.0.0";
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
builder.Services.AddOpenTelemetry()
  .WithTracing(b =>
  {
      b
      .AddAspNetCoreInstrumentation()
      .AddZipkinExporter(config => config.Endpoint = new Uri("http://zipkin:9411/api/v2/spans"))
      .AddSource(serviceName)
      .ConfigureResource(resource =>
          resource.AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion));
  });

var app = builder.Build();

Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithSpan()
                .WriteTo.Seq("http://seq:5341")
                .WriteTo.Console()
                .CreateLogger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/Sub", async (IMessagePublisher _publisher, SubRequest req) =>
{
    Telemetry.ActivitySource.StartActivity("Sub Request recieved", ActivityKind.Consumer);
    //Distributed tracing
    var prop = new TraceContextPropagator();
    var parrentContext = prop.Extract(default, req, (r, key) =>
    {
        return new List<string>(new[] { r.Headers.ContainsKey(key) ? r.Headers[key].ToString() : string.Empty });
    });
    using var activity = Telemetry.ActivitySource.StartActivity("Sub Request recieved", ActivityKind.Consumer, parrentContext.ActivityContext);
    Log.Logger.Information("Subtraction Request recieved {RequestId}", req.guid);
    Baggage.Current = parrentContext.Baggage;
    //Distributed tracing

    var result = Sub(req);
    //Distributed tracing
    var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
    var propContext = new PropagationContext(activityContext, Baggage.Current);
    var propagator = new TraceContextPropagator();
    propagator.Inject(propContext, result, (r, key, value) =>
    {
        r.Headers.Add(key, value);
    });
    //Distributed tracing
    Log.Logger.Information("Calculated result is {Result} for Id: {ResultId}", result.result, req.guid);
    _publisher.PublishSubEvent(result);
    return result;
})
    .WithName("SubtractNumbers")
    .WithOpenApi();

Result Sub(SubRequest req)
{
    using var activity = Telemetry.ActivitySource.StartActivity("Calculating");
    Log.Logger.Information("Calculating..");
    return new Result()
    {
        val1 = req.val1,
        val2 = req.val2,
        operation = req.operation,
        result = req.val1 - req.val2,
        id = req.guid
    };
}

app.Run();

