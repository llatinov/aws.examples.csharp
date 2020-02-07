using System.Threading.Tasks;
using DynamoDbServerless.Models;

namespace DynamoDbServerless.Services
{
    public interface IUserManager
    {
        Task<UserInfo> Authorize(string token);
    }
}