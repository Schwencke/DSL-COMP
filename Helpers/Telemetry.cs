using System.Diagnostics;
using System.Reflection;

namespace Calc_Service_API.Utils
{
    public static class Telemetry
    {
        private static readonly string name = Assembly.GetCallingAssembly().GetName().Name!;
        public static readonly ActivitySource ActivitySource = new ActivitySource(name, "1.0.0");
    }
}
