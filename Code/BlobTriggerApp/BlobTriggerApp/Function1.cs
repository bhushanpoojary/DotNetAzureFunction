using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BlobTriggerApp;

public class Function1
{

    [FunctionName("Function1")]
    public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name, ILogger _logger)
    {
        using var blobStreamReader = new StreamReader(stream);
        var content = await blobStreamReader.ReadToEndAsync();
        _logger.LogInformation("C# Blob Trigger (using Event Grid) processed blob\n Name: {name} \n Data: {content}", name, content);
    }
}
