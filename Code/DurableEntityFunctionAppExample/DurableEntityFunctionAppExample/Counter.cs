using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class Counter
{
    [JsonProperty("value")]
    public int Value { get; set; }

    [FunctionName(nameof(Counter))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
    {
        var current = ctx.GetState<Counter>() ?? new Counter();

        switch (ctx.OperationName.ToLowerInvariant())
        {
            case "add":
                var amount = ctx.GetInput<int>();
                current.Value += amount;
                break;
            case "reset":
                current.Value = 0;
                break;
            case "get":
                ctx.Return(current.Value);
                return Task.CompletedTask;
        }

        ctx.SetState(current);
        return Task.CompletedTask;
    }
}
