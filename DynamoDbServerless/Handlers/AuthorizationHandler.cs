using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DynamoDbServerless.Services;

namespace DynamoDbServerless.Handlers
{
    public class AuthorizationHandler
    {
        private readonly IUserManager _userManager;
        private readonly IJsonConverter _jsonConverter;

        public AuthorizationHandler() : this(null, null)
        {
        }

        public AuthorizationHandler(IJsonConverter jsonConverter, IUserManager userManager)
        {
            _jsonConverter = jsonConverter ?? new JsonConverter();
            _userManager = userManager ?? new UserManager();
        }

        public async Task<APIGatewayCustomAuthorizerResponse> Authorize(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"Query request: {_jsonConverter.SerializeObject(request)}");

            var userInfo = await _userManager.Authorize(request.AuthorizationToken?.Replace("Bearer ", string.Empty));

            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = userInfo.UserId,
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Version = "2012-10-17",
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                    {
                        new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                        {
                            Action = new HashSet<string> {"execute-api:Invoke"},
                            Effect = userInfo.Effect.ToString(),
                            Resource = new HashSet<string> { request.MethodArn }
                        }
                    }
                }
            };
        }
    }
}