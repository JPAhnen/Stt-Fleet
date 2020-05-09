using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using STTFleet.STTApi;

namespace STTFleet
{
    public static class FleetEventRanks
    {
        [FunctionName("FleetEventRanks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue("fleeteventranks"), StorageAccount("sttfleetg5618_STORAGE")] ICollector<string> msg,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Fleet event ranks started");
            var configuration = new Configuration(context.FunctionAppDirectory);
            var gameApi = new GameApi(configuration.STTApiConfiguration);
            await gameApi.Login();
            var data = await gameApi.GetFleetMemberInfo();
            var userdailies = UserDailies.Load(data);
            var squadranks = SquadEventRank.Load(data);

            log.LogInformation("Fleet event ranks users: " + userdailies.Count);

            var queueItem = new EventRankQueueItem() { UserDailies = userdailies, SquadEventRanks = squadranks };

            msg.Add(JsonConvert.SerializeObject(queueItem));

            return new OkResult();
        }
    }
}
