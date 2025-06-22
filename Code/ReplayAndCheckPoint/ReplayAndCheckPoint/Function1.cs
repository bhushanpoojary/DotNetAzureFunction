using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace ReplayAndCheckPoint;

public static class Function1
{
    [Function(nameof(Function1))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger(nameof(Function1));
        var outputs = new List<string>();

        // ⚠️ Replay-safe time (DO NOT use DateTime.Now here)
        var orchestrationTime = context.CurrentUtcDateTime;

        if (!context.IsReplaying)
        {
            logger.LogInformation("=== Orchestration started at {time} ===", orchestrationTime);
        }

        if (context.IsReplaying)
            Console.WriteLine("🔁 This is a replay (Console log shown even in replay)");
        else
            Console.WriteLine("🟢 First Execution");

        // ✅ First checkpoint: Activity "Tokyo"
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
        if (!context.IsReplaying) 
            logger.LogInformation("Checkpointed after Tokyo.");

        // ✅ Second checkpoint: Activity "Seattle"
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
        if (!context.IsReplaying) 
            logger.LogInformation("Checkpointed after Seattle.");

        // ✅ Third checkpoint: Activity "London"
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));
        if (!context.IsReplaying) 
            logger.LogInformation("Checkpointed after London.");

        if (!context.IsReplaying)
        {
            logger.LogInformation("=== Orchestration completed ===");
        }

        return outputs;
    }

    [Function(nameof(SayHello))]
    public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("SayHello");
        logger.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }

    [Function("Function1_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("Function1_HttpStart");

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(Function1));

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}