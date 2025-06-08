using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzurefunctionsApp
{
    public class FunctionTimerTriggerExample
    {
        [FunctionName("FunctionTimerTriggerExample")]
        public void Run([TimerTrigger("*/1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
