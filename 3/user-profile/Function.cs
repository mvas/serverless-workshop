using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace UserProfile
{
    public class Function
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest gatewayEvent, ILambdaContext context)
        {
            var authHeaderKey = Environment.GetEnvironmentVariable("AUTH_HEADER");
            var accessTokenKey = Environment.GetEnvironmentVariable("ACCESS_TOKEN_PARAM_NAME");

            if ( gatewayEvent == null ||
                gatewayEvent.Headers == null || 
                gatewayEvent.QueryStringParameters == null )
            {
                return GenerateErrorResponse("malformed API event");
            }

            if (!gatewayEvent.Headers.ContainsKey(authHeaderKey))
            {
                return GenerateErrorResponse("ID token not found");
            }
            
            if (!gatewayEvent.QueryStringParameters.ContainsKey(accessTokenKey))
            {
                return GenerateErrorResponse("AccessToken not found");
            }

            var idToken = gatewayEvent.Headers[authHeaderKey];
            var accessToken = gatewayEvent.QueryStringParameters[accessTokenKey];
            var auth0Domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");

            var url = $"https://{auth0Domain}/userinfo";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return GenerateResponse(200, await response.Content.ReadAsStringAsync());
            }
            else
            {
                return GenerateResponse(400, await response.Content.ReadAsStringAsync());
            }
        }

        private APIGatewayProxyResponse GenerateErrorResponse(string errorMessage)
        {
            return GenerateResponse(400, $"\"{errorMessage}\"");
        }

        private APIGatewayProxyResponse GenerateResponse(int code, string content)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = code,
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" }
                },
                Body = $"{{\"message\":{content}}}"
            };
        }
    }
}
