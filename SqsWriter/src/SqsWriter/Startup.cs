using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqsWriter.HealthChecks;
using SqsWriter.Middleware;
using SqsWriter.Sqs;
using Serilog;

namespace SqsWriter
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
            services.AddHealthChecks()
                .AddCheck<SqsHealthCheck>("SQS Health Check");
        }

        public void Configure(
            IApplicationBuilder app
            , ISqsClient sqsClient
            )
        {
            app.UseSerilogRequestLogging();
            app.UseMiddleware<HttpExceptionMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustomHealthChecks("SqsWriter service");
            });

            if (_appConfig.AwsSettings.AutomaticallyCreateQueue)
            {
                sqsClient.CreateQueue().Wait();
            }
        }
    }
}
