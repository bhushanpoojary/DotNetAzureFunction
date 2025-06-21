using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading;

namespace DurableTaskFrameworkBasics
{
    public static class Function1
    {
        [FunctionName("MyOrchestrator")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            string name = context.GetInput<string>();

            string greeting = await context.CallActivityAsync<string>("SayHello", name);
            string timeInfo = await context.CallActivityAsync<string>("GetTime", null);
            string summary = await context.CallActivityAsync<string>("CreateSummary", $"{greeting} at {timeInfo}");

            return summary;
        }

        [FunctionName("SayHello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}");
            return $"Hello, {name}";
        }

        [FunctionName("GetTime")]
        public static string GetTime([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"Waiting for 15 seconds");
            Thread.Sleep(15000);
            string time = DateTime.UtcNow.ToString("HH:mm:ss");
            log.LogInformation($"Fetched time: {time}");
            return time;
        }

        [FunctionName("CreateSummary")]
        public static string CreateSummary([ActivityTrigger] string message, ILogger log)
        {
            log.LogInformation($"Creating summary: {message}");
            return $"Summary: {message}";
        }

        [FunctionName("HttpStart")]
        public static async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
        [DurableClient] IDurableOrchestrationClient starter,
         ILogger log)
        {
            string name = req.Query["name"];

            if (string.IsNullOrWhiteSpace(name))
            {
                return new BadRequestObjectResult("Please pass a name on the query string.");
            }

            // Start orchestration with 'name' as input
            string instanceId = await starter.StartNewAsync("MyOrchestrator", input: name);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
