using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using SqsWriter.HealthChecks;
using SqsWriter.Middleware;
using SqsWriter.Sqs;

namespace SqsWriter
{
    public class Startup
    {
        private readonly AppConfig _appConfig = new AppConfig();
        public Startup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();
            Configuration.Bind(_appConfig);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddNewtonsoftJson(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);
            services.Configure<AppConfig>(Configuration);
            services.AddLogging(x => x.AddFilter("Microsoft", LogLevel.Warning));
            services.AddSingleton<IAmazonSQS>(x => SqsClientFactory.CreateClient(_appConfig));
            services.AddSingleton<ISqsClient, SqsClient>();
            services.AddHealthChecks()
                .AddCheck<SqsHealthCheck>("SQS Health Check");
        }

        public async void Configure(IApplicationBuilder app, ISqsClient sqsClient)
        {
            app.UseSerilogRequestLogging();
            app.UseMiddleware<HttpExceptionMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustomHealthChecks("SqsWriter service");
            });

            if (_appConfig.AwsQueueAutomaticallyCreate)
            {
                await sqsClient.CreateQueueAsync();
            }
        }
    }
}
