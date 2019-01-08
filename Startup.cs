using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SlimMessageBus;
using SlimMessageBus.Host.AspNetCore;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Kafka;
using SlimMessageBus.Host.Kafka.Configs;
using SlimMessageBus.Host.Serialization.Json;

namespace SimpeConsumerSMBKafka
{
    public class Startup
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Startup));

        public Startup(IConfiguration config)
        {
            Configuration = config;

            var logConfiguration = new LogConfiguration();
            config.GetSection("LogConfiguration").Bind(logConfiguration);
            LogManager.Configure(logConfiguration);
        }

        public IConfiguration Configuration { get; }
            
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            ConfigureMessageBus(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });

            ConfigureMessageBus(app);
        }

        public void ConfigureMessageBus(IApplicationBuilder app)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, "Register the bus");
            // Set the MessageBus provider, so that IMessageBus are resolved from the current request scope
            MessageBus.SetProvider(MessageBusCurrentProviderBuilder.Create().FromPerRequestScope(app).Build());
        }

        public void ConfigureMessageBus(IServiceCollection services)
        {
            services.AddScoped<RecognizeImageRequestHandler>();

            Log.InfoFormat(CultureInfo.InvariantCulture, "Instanciando the bus");
            var messageBusBuilder = MessageBusBuilder.Create()
                                    .SubscribeTo<RecognizeImageRequest>(x => x
                                        .Topic("image-to-recognize")
                                        .Group("workers")
                                        .WithSubscriber<RecognizeImageRequestHandler>())
                                    .WithSerializer(new JsonMessageSerializer())
                                    .WithProviderKafka(new KafkaMessageBusSettings(Configuration["Kafka:Brokers"]));

            // Make the MessageBus per request scope
            services.AddScoped<IMessageBus>(svp => messageBusBuilder
                .WithDependencyResolver(new AspNetCoreMessageBusDependencyResolver(svp.GetService<IHttpContextAccessor>()))
                .Build());
        }
    }
}
