using Calc_Service_API.Data;
using EasyNetQ;
using Events;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Serilog;
using System.Diagnostics;

namespace Calc_Service_API.Utils
{
    public class MessageListenerService
    {
        private readonly IServiceProvider _provider;
        private readonly string _connString;


        public MessageListenerService(IServiceProvider serviceProvider, string connectionString)
        {
            _provider = serviceProvider;
            _connString = connectionString;
        }

        public void Start()
        {

            using (var bus = RabbitHutch.CreateBus(_connString))
            {

                var a = bus.PubSub.SubscribeAsync<Result>("CalcAPI", LogHistory, x => x.WithTopic("Result"));
                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        void LogHistory(Result r)
        {
            //Distributed tracing
            var prop = new TraceContextPropagator();
            var parrentContext = prop.Extract(default, r, (r, key) =>
            {
                return new List<string>(new[] { r.Headers.ContainsKey(key) ? r.Headers[key].ToString() : string.Empty });
            });
            using (var activity = Telemetry.ActivitySource.StartActivity("Logging history", ActivityKind.Consumer, parrentContext.ActivityContext))
            {

                Log.Logger.Information("Addition Request recieved {ResultGUID}", r.id);
                Baggage.Current = parrentContext.Baggage;
                //Distributed tracing
                activity?.SetTag("Calculation result", r.result);
                activity?.SetTag("Operation", r.operation);

                using (var subActivity = Telemetry.ActivitySource.StartActivity("Saving to DB", ActivityKind.Internal, parrentContext.ActivityContext))
                {
                    Log.Logger.Information("Logging a result to the database");
                    using (var scope = _provider.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        var repos = services.GetService<IRepository<Result>>();
                        repos.Add(r);

                    }

                }

            }

        }
    }
}
