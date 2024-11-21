using System.Text;
using System.Net.Http;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(object input, ILambdaContext context)
        {
            context.Logger.LogInformation($"FunctionHandler received: {input}");

            // Deserialize the input JSON dynamically
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
            string payload = $"{{'text':'Issue Created: {json.issue.html_url}'}}";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            try
            {
                var response = client.Send(request);
                response.EnsureSuccessStatusCode();
                using var reader = new StreamReader(response.Content.ReadAsStream());
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Failed to send Slack notification: {ex.Message}");
                throw;
            }
        }
    }
}