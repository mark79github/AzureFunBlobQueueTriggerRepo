using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunApp2TableStoragewithQueueBlobTriggers
{
    public static class WriteUserProfilePicFromQueueTrigger2BlobFun
    {
        [FunctionName("WriteUserProfilePicFromQueueTrigger2BlobFun")]
        public static async Task Run(
            [Blob("userprofileimagecontainer/{rand-guid}.jpg", FileAccess.Write)] CloudBlockBlob outputBlob,
            [QueueTrigger("userprofileimagequeue", Connection = "AzureWebJobsStorage")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation("C# Queue trigger function started processing a message.");
            byte[] imageData = null;
            using (var wc = new System.Net.WebClient())
            {
                imageData = wc.DownloadData(myQueueItem);
            }
            await outputBlob.UploadFromByteArrayAsync(imageData, 0, imageData.Length);
            log.LogInformation("C# Queue trigger function processed a message.");
        }
    }
}
