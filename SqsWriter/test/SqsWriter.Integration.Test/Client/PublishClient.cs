using System.Net.Http;
using System.Threading.Tasks;
using SqsWriter.Sqs.Models;

namespace SqsWriter.Integration.Test.Client
{
    public class PublishClient : BaseClient
    {
        public PublishClient(HttpClient httpClient)
            : base(httpClient) { }

        public async Task<ApiResponse<string>> PublishMovie(Movie movie)
        {
            return await SendAsync<string>(HttpMethod.Post, "api/publish/movie", movie);
        }

        public async Task<ApiResponse<string>> PublishActor(Actor actor)
        {
            return await SendAsync<string>(HttpMethod.Post, "api/publish/actor", actor);
        }
    }
}