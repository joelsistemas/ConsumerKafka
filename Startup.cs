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
using SimpeConsumerSMBKafka.Services;
using SlimMessageBus;
using SlimMessageBus.Host.AspNetCore;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Kafka;
using SlimMessageBus.Host.Kafka.Configs;
using SlimMessageBus.Host.Serialization.Json;
using System;
using System.Globalization;

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
            //services.Add(ServiceDescriptor.Singleton<IPeopleService, PeopleService>());
            services.AddTransient<IPeopleService, PeopleService>();

            services.AddHttpClient();

            services.AddTransient<RecognizeImageRequestHandler>();
            
            //Message Bus
            services.AddSingleton<IMessageBus>(BuildMessageBus);
            services.AddSingleton<IRequestResponseBus>(svp => svp.GetService<IMessageBus>());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
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

            // Force the singleton SMB instance to be created on app start rather than when requested.
            // We want message to be consumed when right away when WebApi starts (more info https://stackoverflow.com/a/39006021/1906057)
            var messageBus = app.ApplicationServices.GetService<IMessageBus>();        
        }

        private IMessageBus BuildMessageBus(IServiceProvider serviceProvider)
        {

            Log.InfoFormat(CultureInfo.InvariantCulture, "Instanciando the bus");

            var messageBusBuilder = MessageBusBuilder.Create()
                                    .SubscribeTo<RecognizeImageRequest>(x => x
                                        .Topic("image-to-recognize")
                                        .Group("workers")
                                        .Instances(3)
                                        .WithSubscriber<RecognizeImageRequestHandler>())
                                    .WithDependencyResolverAsAspNetCore(serviceProvider)
                                    .WithSerializer(new JsonMessageSerializer())
                                    .WithProviderKafka(new KafkaMessageBusSettings(Configuration["Kafka:Brokers"]));

            var messageBus = messageBusBuilder.Build();
            return messageBus;
        }
    }
}
