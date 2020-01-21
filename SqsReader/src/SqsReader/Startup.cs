using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using SqsReader.Dynamo;
using SqsReader.HealthChecks;
using SqsReader.Services;
using SqsReader.Services.Processors;
using SqsReader.Sqs;

namespace SqsReader
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
            services.AddSingleton<ISqsConsumerService, SqsConsumerService>();

            services.AddSingleton<IAmazonDynamoDB>(x => DynamoDbClientFactory.CreateClient(_appConfig));
            services.AddSingleton<IDatabaseClient, DatabaseClient>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            services.AddSingleton<IActorsRepository, ActorsRepository>();
            services.AddSingleton<IMoviesRepository, MoviesRepository>();

            services.AddScoped<IMessageProcessor, ActorMessageProcessor>();
            services.AddScoped<IMessageProcessor, MovieMessageProcessor>();

            services.AddHealthChecks()
                .AddCheck<SqsHealthCheck>("SQS Health Check");
        }

        public async void Configure(
            IApplicationBuilder app,
            ISqsClient sqsClient,
            ISqsConsumerService sqsConsumerService,
            IActorsRepository actorsRepository,
            IMoviesRepository moviesRepository)
        {
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustomHealthChecks("SqsReader service");
            });

            if (_appConfig.AwsQueueAutomaticallyCreate)
            {
                await sqsClient.CreateQueue();
            }

            sqsConsumerService.StartConsuming();
            await actorsRepository.CreateTableAsync();
            await moviesRepository.CreateTableAsync();
        }
    }
}
