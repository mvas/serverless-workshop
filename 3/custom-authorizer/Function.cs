using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading;
using Microsoft.IdentityModel.Tokens;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CustomAuthorizer
{
    public class Function
    {
        public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest authEvent, ILambdaContext context)
        {
            if (string.IsNullOrEmpty(authEvent.AuthorizationToken))
            {
                Console.WriteLine("no token passed");
                return null;
            }

            var tokenParts = authEvent.AuthorizationToken.Split(' ');
            var jwtTokenString = tokenParts.Length > 1 ? tokenParts[1] : null;
            if (string.IsNullOrEmpty(jwtTokenString))
            {
                Console.WriteLine("no jwt token found");
                return null;
            }

            var auth0Domain = System.Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
            var auth0Audience = System.Environment.GetEnvironmentVariable("AUTH0_AUDIENCE");

            if (string.IsNullOrEmpty(auth0Domain) || string.IsNullOrEmpty(auth0Audience))
            {
                Console.WriteLine("no authroziation config found");
                return null;
            }


            Console.WriteLine("domain:" + auth0Domain);
            Console.WriteLine("audience:" + auth0Audience);
            Console.WriteLine("token:" + jwtTokenString);

            var configUrl = $"https://{auth0Domain}/.well-known/openid-configuration";
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                configUrl, new OpenIdConnectConfigurationRetriever());
            var openIdConfig = await configManager.GetConfigurationAsync(CancellationToken.None);

            var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = $"https://{auth0Domain}/",
                    ValidAudiences = new[] { auth0Audience },
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

            SecurityToken validatedToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                handler.ValidateToken(jwtTokenString, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException)
            {
                Console.WriteLine("Failed authorization " + authEvent.AuthorizationToken);
                return null;
            }
            if (validatedToken == null)
            {
                Console.WriteLine("Failed authorization " + authEvent.AuthorizationToken);
                return null;
            }
            return GenerateAllowResponse(authEvent.MethodArn);
        }

        private APIGatewayCustomAuthorizerResponse GenerateAllowResponse(string resource)
        {
            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = "user",
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Version = "2012-10-17",
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                            {
                                new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                                {
                                    Action = new HashSet<string> { "execute-api:Invoke" },
                                    Effect = "allow",
                                    Resource = new HashSet<string> { resource }
                                }
                            }
                }
            };
        }
    }
}
