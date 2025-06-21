using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public static class HttpStart
{
    [FunctionName("HttpAdd")]
    public static async Task<IActionResult> Add(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
        [DurableClient] IDurableEntityClient client)
    {
        var entityId = new EntityId("Counter", "myCounter");
        await client.SignalEntityAsync(entityId, "add", 5);
        return new OkObjectResult("Added 5 to counter.");
    }

    [FunctionName("HttpGet")]
    public static async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        [DurableClient] IDurableEntityClient client)
    {
        var entityId = new EntityId("Counter", "myCounter");
        var state = await client.ReadEntityStateAsync<Counter>(entityId);
        return new OkObjectResult($"Counter value: {(state.EntityState.Value != 0 ? state.EntityState.Value.ToString() : "not found")}");
    }

    [FunctionName("HttpReset")]
    public static async Task<IActionResult> Reset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        [DurableClient] IDurableEntityClient client)
    {
        var entityId = new EntityId("Counter", "myCounter");
        await client.SignalEntityAsync(entityId, "reset");
        return new OkObjectResult("Counter reset.");
    }
}
