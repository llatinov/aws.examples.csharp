using System.Threading.Tasks;
using SqsReader.Sqs.Models;

namespace SqsReader.Dynamo
{
    public interface IActorsRepository
    {
        Task CreateTableAsync();

        Task SaveActorAsync(Actor actor);
    }
}