using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace FunApp2TableStoragewithQueueBlobTriggers
{
    public static class RegisterUserHTTPTrigger2TableAndQueueFun
    {
        [FunctionName("RegisterUserHTTPTrigger2TableAndQueueFun")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [Queue("UserProfileImageQueue")] IAsyncCollector<string> objUserProfileQueueItem,
        [Table("userprofile")] CloudTable objUserProfileTable,
        ILogger log)
        {
            // here we are simply adding one table (user entity) into Table storage but we can also
            // parse the Http Request req and filtre the operations in a switch case depending on the 
            // URI parsing mechanism.

            log.LogInformation("C# HTTP trigger function started processing a request.");
            string firstname = null, lastname = null;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic inputJson = JsonConvert.DeserializeObject(requestBody);
            firstname = firstname ?? inputJson?.firstname;
            lastname = inputJson?.lastname;

            string profilePicUrl = inputJson?.ProfilePicUrl ?? null;
            await objUserProfileQueueItem.AddAsync(profilePicUrl);

            UserProfile objUserProfile = new UserProfile(firstname, lastname);
            TableOperation objTblOperationInsert = TableOperation.Insert(objUserProfile);
            await objUserProfileTable.ExecuteAsync(objTblOperationInsert);
            return (lastname + firstname) != null
            ? (ActionResult)new OkObjectResult($"Hello, {firstname} {lastname} Your profile URL is, {profilePicUrl}")
            : new BadRequestObjectResult("Please pass a name on the query" +
            "string or in the request body");
        }

        public class UserProfile : TableEntity
        {
            public UserProfile(string firstName, string lastName)
            {
                this.PartitionKey = "p1";
                this.RowKey = Guid.NewGuid().ToString();
                this.FirstName = firstName;
                this.LastName = lastName;
            }
            public UserProfile() { }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
