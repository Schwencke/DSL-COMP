using Calc_Service_API.Data;
using Calc_Service_API.Utils;
using Events;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Serilog;
using Serilog.Enrichers.Span;
using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var serviceName = Assembly.GetCallingAssembly().GetName().Name;
var serviceVersion = "1.0.0";

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


// Add services to the container.
builder.Services.AddDbContext<ResultContext>(opt => opt.UseInMemoryDatabase("ResultsDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Result>, ResultRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("AddClient", client => client.BaseAddress = new Uri("http://add-service/"))
    .AddTransientHttpErrorPolicy(
    policyBuilder => policyBuilder.WaitAndRetryAsync(
                    Backoff.DecorrelatedJitterBackoffV2(
                        TimeSpan.FromSeconds(1), 5)))
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.CircuitBreakerAsync(2, TimeSpan.FromSeconds(10)));

builder.Services.AddHttpClient("SubClient", client => client.BaseAddress = new Uri("http://sub-service/"))
    .AddTransientHttpErrorPolicy(
    policyBuilder => policyBuilder.WaitAndRetryAsync(
                    Backoff.DecorrelatedJitterBackoffV2(
                        TimeSpan.FromSeconds(1), 5)))
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.CircuitBreakerAsync(2, TimeSpan.FromSeconds(10)));

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

// Initialize the database.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<ResultContext>();
    var dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

app.MapPost("/addition", async Task<IResult> (AddRequest req, IHttpClientFactory factory) =>
{

    using var activity = Telemetry.ActivitySource.StartActivity("addition");
    Log.Logger.Information("Got addition request: {RequestGUID}", req.guid);
    Log.Logger.Debug("Adding numbers");
    var client = factory.CreateClient("AddClient");
    using var sendingRequest = Telemetry.ActivitySource.StartActivity("Sending request to subtraction service", ActivityKind.Producer);
    Log.Logger.Information("Sending request to addition service");
    //Distributed tracing
    var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
    var propContext = new PropagationContext(activityContext, Baggage.Current);
    var propagator = new TraceContextPropagator();
    propagator.Inject(propContext, req, (r, key, value) =>
    {
        r.Headers.Add(key, value);
    });
    //Distributed tracing
    HttpResponseMessage response = null;
    Result result = null;
    try
    {
        response = await client.PostAsJsonAsync("Add", req);
        response.EnsureSuccessStatusCode();
        result = await response.Content.ReadFromJsonAsync<Result>();
    }
    catch (HttpRequestException e)
    {
        return Results.Problem(e.Message, null, 503);
    }
    if (response is null || result is null)
    {
        return Results.Problem("There was a problem processing your request", null, 500);
    }


    //Distributed tracing
    var prop = new TraceContextPropagator();
    var parrentContext = prop.Extract(default, result, (r, key) =>
    {
        return new List<string>(new[] { r.Headers.ContainsKey(key) ? r.Headers[key].ToString() : string.Empty });
    });
    Baggage.Current = parrentContext.Baggage;
    using var activitys = Telemetry.ActivitySource.StartActivity("Result recieved", ActivityKind.Consumer, parrentContext.ActivityContext);
    Log.Logger.Information("Result recieved with {ResultId}", result.id);
    //Distributed tracing

    return Results.Ok(result);

})
    .WithName("AddNumbers")
    .WithOpenApi();


app.MapPost("/subtraction", async Task<IResult> (SubRequest req, IHttpClientFactory factory) =>
{
    using var activity = Telemetry.ActivitySource.StartActivity("subtraction");
    Log.Logger.Information("Got subtracting request: {RequestGUID}", req.guid);
    var client = factory.CreateClient("SubClient");
    using var sendingRequest = Telemetry.ActivitySource.StartActivity("Sending request to subtraction service", ActivityKind.Producer);
    Log.Logger.Information("Sending request to subtraction service");
    //Distributed tracing
    var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
    var propContext = new PropagationContext(activityContext, Baggage.Current);
    var propagator = new TraceContextPropagator();
    propagator.Inject(propContext, req, (r, key, value) =>
    {
        r.Headers.Add(key, value);
    });
    //Distributed tracing
    HttpResponseMessage response = null;
    Result result = null;
    try
    {
        response = await client.PostAsJsonAsync("Sub", req);
        response.EnsureSuccessStatusCode();
        result = await response.Content.ReadFromJsonAsync<Result>();
    }
    catch (HttpRequestException e)
    {
        return Results.Problem(e.Message, null, 503);
    }
    if (response is null || result is null)
    {
        return Results.Problem("There was a problem processing your request", null, 500);
    }

    //Distributed tracing
    var prop = new TraceContextPropagator();
    var parrentContext = prop.Extract(default, result, (r, key) =>
    {
        return new List<string>(new[] { r.Headers.ContainsKey(key) ? r.Headers[key].ToString() : string.Empty });
    });
    Baggage.Current = parrentContext.Baggage;
    using var activitys = Telemetry.ActivitySource.StartActivity("Result recieved", ActivityKind.Consumer, parrentContext.ActivityContext);
    Log.Logger.Information("Result recieved with {ResultId}", result.id);
    //Distributed tracing
    return Results.Ok(result);


})
    .WithName("SubtractNumbers")
    .WithOpenApi();

app.MapGet("/", (IRepository<Result> rep) =>
{
    using (var activity = Telemetry.ActivitySource.StartActivity("gettinghistory"))
    {
        Log.Logger.Information("Getting history");
        return rep.GetAll();
    }
});


// Ensure a messagelistner is started on a seperate thread
Task.Factory.StartNew(() =>
    new MessageListenerService(app.Services, "host=rabbitmq").Start());


app.Run();