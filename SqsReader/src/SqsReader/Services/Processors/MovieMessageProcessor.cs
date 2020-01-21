using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using SqsReader.Dynamo;

namespace SqsReader.Services.Processors
{
    public class MovieMessageProcessor : IMessageProcessor
    {
        private readonly IMoviesRepository _moviesRepository;
        private readonly ILogger<MovieMessageProcessor> _logger;

        public MovieMessageProcessor(IMoviesRepository moviesRepository, ILogger<MovieMessageProcessor> logger)
        {
            _moviesRepository = moviesRepository;
            _logger = logger;
        }

        public bool CanProcess(string messageType)
        {
            return messageType == typeof(Movie).Name;
        }

        public async Task ProcessAsync(Message message)
        {
            _logger.LogInformation($"MovieMessageProcessor invoked with: {message.Body}");
            var movie = JsonConvert.DeserializeObject<Movie>(message.Body);
            await _moviesRepository.SaveMovieAsync(movie);
        }
    }
}