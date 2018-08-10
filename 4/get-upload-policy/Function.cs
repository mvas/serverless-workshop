using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetUploadPolicy
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            // Get the filename from the query string parameters in the GET call
            var filename = WebUtility.UrlDecode(input.QueryStringParameters["filename"]);
            var directory = Guid.NewGuid().ToString();

            var key = directory + '/' + filename;
            var bucket = Environment.GetEnvironmentVariable("UPLOAD_BUCKET");

            var policyDocument = GeneratePolicyDocument(key, bucket);
            var encodedPolicyDocument = Encode(policyDocument);
            var signature = GenerateSignature(encodedPolicyDocument);

            var body = new
            {
                signature = signature,
                encoded_policy = encodedPolicyDocument,
                access_key = Environment.GetEnvironmentVariable("ACCESS_KEY_ID"),
                upload_url = "https://" + bucket + ".s3.amazonaws.com/",
                key = key
            };

            return GenerateResponse(200, body);
        }

        private APIGatewayProxyResponse GenerateResponse(int code, object content)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = code,
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" }
                },
                Body = JsonConvert.SerializeObject(content)
            };
        }
        
        private string GeneratePolicyDocument(string key, string bucket)
        {
            var expiration = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.sssZ");

            return "{" +
                $"\"expiration\": \"{expiration}\"," +
                "\"conditions\": [" +
                $"{{\"key\": \"{key}\"}}," +
                $"{{\"bucket\": \"{bucket}\"}}," +
                "{\"acl\": \"private\"}," +
                "[\"starts-with\", \"$Content-Type\", \"\"]]}";
        }
        private string Encode(string msg)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        private string GenerateSignature(string encodedText)
        {
            var key = Environment.GetEnvironmentVariable("SECRET_ACCESS_KEY");
            var hash = new System.Security.Cryptography.HMACSHA1(new ASCIIEncoding().GetBytes(key));
            return Convert.ToBase64String(hash.ComputeHash(new ASCIIEncoding().GetBytes(encodedText)));
        }

    }
}
