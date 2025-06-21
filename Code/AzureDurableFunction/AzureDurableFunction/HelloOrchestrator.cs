using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureDurableFunction
{
    public static class HelloOrchestrator
    {
        [FunctionName("OrchestrateHello")]
        public static async Task<List<string>> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>("SayHello", "Alice"));
            outputs.Add(await context.CallActivityAsync<string>("SayHello", "Bob"));
            outputs.Add(await context.CallActivityAsync<string>("SayHello", "Charlie"));

            return outputs;
        }
    }
}