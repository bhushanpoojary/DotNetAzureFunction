using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RandomFailureFunctionApp
{
    public static class RandomErrorFunction
    {
        [FunctionName("RandomErrorFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            int value = (new Random()).Next(1, 5);

            log.LogInformation($"Function triggered with value: {value}");

            if (value % 2 == 0)
            {
                log.LogError("Something went wrong! Throwing exception...");
                throw new Exception($"Simulated error with value: {value}");
            }
            return new OkObjectResult("Function executed successfully");
        }
    }
}
