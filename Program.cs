using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using Microsoft.Extensions.Logging;

namespace SimpeConsumerSMBKafka
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                                .ConfigureLogging(logging =>
                                {
                                    logging.ClearProviders();
                                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                                })
                .UseNLog();
    }
}
