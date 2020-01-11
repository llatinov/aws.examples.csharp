using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqsReader.HealthChecks;
using SqsReader.Middleware;
using SqsReader.Services;
using SqsReader.Services.Processors;
using SqsReader.Sqs;
using Serilog;

namespace SqsReader
{
    public class Startup
    {
        private readonly AppConfig _appConfig = new AppConfig();
        public Startup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();
            Configuration.Bind(_appConfig);
            _appConfig.AwsSettings.UpdateFromEnvironment();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddNewtonsoftJson(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);
            services.Configure<AppConfig>(Configuration);
            services.AddLogging(x => x.AddFilter("Microsoft", LogLevel.Warning));
            services.AddSingleton<IAmazonSQS>(x => SqsClientFactory.CreateClient(_appConfig.AwsSettings));
            services.AddSingleton<ISqsClient, SqsClient>();
            services.AddSingleton<ISqsConsumerService, SqsConsumerService>();
            services.AddScoped<IMessageProcessor, ActorMessageProcessor>();
            services.AddScoped<IMessageProcessor, MovieMessageProcessor>();
            services.AddHealthChecks()
                .AddCheck<SqsHealthCheck>("SQS Health Check");
        }

        public void Configure(
            IApplicationBuilder app
            , ISqsClient sqsClient
            , ISqsConsumerService sqsConsumerService
            )
        {
            app.UseSerilogRequestLogging();
            app.UseMiddleware<HttpExceptionMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustomHealthChecks("SqsReader service");
            });

            if (_appConfig.AwsSettings.AutomaticallyCreateQueue)
            {
                sqsClient.CreateQueue().Wait();
            }
            sqsConsumerService.StartConsuming();
        }
    }
}
