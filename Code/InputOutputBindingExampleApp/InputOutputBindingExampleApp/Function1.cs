using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace InputOutputBindingExampleApp
{
    public class Function1
    {
        [FunctionName("QueueToBlobFunction")]
        public static async Task Run(
            [QueueTrigger("input-queue", Connection = "AzureWebJobsStorage")] string queueMessage,
            [Blob("output-container/{rand-guid}.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outputBlob,
            ILogger log)
        {
            log.LogInformation($"📥 Received queue message: {queueMessage}");
            var bytes = Encoding.UTF8.GetBytes($"Processed message: {queueMessage}");
            await outputBlob.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
