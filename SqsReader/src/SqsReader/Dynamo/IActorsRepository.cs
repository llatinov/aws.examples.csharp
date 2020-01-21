using System.Threading.Tasks;
using Models;

namespace SqsReader.Dynamo
{
    public interface IActorsRepository
    {
        Task CreateTableAsync();

        Task SaveActorAsync(Actor actor);
    }
}