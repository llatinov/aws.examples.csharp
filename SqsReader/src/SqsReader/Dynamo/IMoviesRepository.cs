using System.Threading.Tasks;
using Models;

namespace SqsReader.Dynamo
{
    public interface IMoviesRepository
    {
        Task CreateTableAsync();

        Task SaveMovieAsync(Movie actor);
    }
}