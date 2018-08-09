using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.ElasticTranscoder;
using Newtonsoft.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MyFunc
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<S3Event> FunctionHandler(S3Event input, ILambdaContext context)
        {
            //todo: tmp
            var inputStr = Newtonsoft.Json.JsonConvert.SerializeObject(input);
            Console.WriteLine(inputStr);

            if (input.Records.Count == 0)
            {
                return input;
            }

            //do we need to specify a region??            
            string pipelineId = "1533763792153-jhnsky";
            var key = input.Records[0].S3.Object.Key;

            // decode
            var sourceKey = System.Net.WebUtility.UrlDecode(key.Replace('+', ' '));

            //remove file extension
            var outputKey = sourceKey.Split('.')[0];


            var request = new Amazon.ElasticTranscoder.Model.CreateJobRequest {
                PipelineId = pipelineId,
                OutputKeyPrefix = outputKey + "/",
                Input = new Amazon.ElasticTranscoder.Model.JobInput {
                    Key = sourceKey
                },
                Outputs = {
                    new Amazon.ElasticTranscoder.Model.CreateJobOutput {
                        Key = outputKey + "-1080p" + ".mp4",
                        PresetId = "1351620000001-000001" //Generic 1080p
                    },
                    new Amazon.ElasticTranscoder.Model.CreateJobOutput{
                        Key = outputKey + "-720p" + ".mp4",
                        PresetId = "1351620000001-000010" //Generic 720p
                    },
                    new Amazon.ElasticTranscoder.Model.CreateJobOutput{
                        Key = outputKey + "-web-720p" + ".mp4",
                        PresetId = "1351620000001-100070" //Web Friendly 720p
                    }
                }
            };

            var client = new Amazon.ElasticTranscoder.AmazonElasticTranscoderClient();
            await client.CreateJobAsync(request);
            
            return input;
        }
    }
}
