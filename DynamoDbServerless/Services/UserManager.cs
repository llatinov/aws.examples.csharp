using System.Threading.Tasks;
using DynamoDbServerless.Models;

namespace DynamoDbServerless.Services
{
    public class UserManager : IUserManager
    {
        private const string ValidToken = "validToken";
        private const string UserId = "usedId";

        public async Task<UserInfo> Authorize(string token)
        {
            var userInfo = new UserInfo
            {
                UserId = UserId,
                Effect = token == ValidToken ? EffectType.Allow : EffectType.Deny
            };

            return userInfo;
        }
    }
}