using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetryOnFailureApp
{
    public static class FunctionWithRetry
    {
        private static readonly Random _random = new Random();
        [FunctionName("FunctionWithRetry")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(5),
                maxNumberOfAttempts: 3)
            {
                BackoffCoefficient = 2.0,
                Handle = (ex) =>
                {
                    // ✅ Retry only if it's an InvalidOperationException
                    return ex is InvalidOperationException;
                }
            };
            outputs.Add(await context.CallActivityWithRetryAsync<string>(nameof(FailingActivity), retryOptions, "Fail Safe"));
            return outputs;
        }

        [FunctionName("FailingActivity")]
        public static string FailingActivity([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Activity triggered for name: {name}");

            // Simulate random failure 50% of the time
            int chance = _random.Next(1, 11); // 1 to 10
            if (chance <= 5)
            {
                log.LogWarning("❌ Simulated random failure!");
                throw new InvalidOperationException("Random failure occurred.");
            }

            log.LogInformation("✅ Success on this attempt.");
            return $"Hello, {name}!";
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FunctionWithRetry", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}