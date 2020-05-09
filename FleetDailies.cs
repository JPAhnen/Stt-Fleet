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
using System.Text;
using System.Net.Http;
using System.Linq;

namespace STTFleet
{
    public static class FleetDailies
    {
        [FunctionName("FleetDailies")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Fleet dailies started");
            var configuration = new Configuration(context.FunctionAppDirectory);
            var gameApi = new GameApi(configuration.STTApiConfiguration);
            await gameApi.Login();
            var data = await gameApi.GetFleetMemberInfo();
            var fleet = UserDailies.Load(data);

            log.LogInformation("Fleet dailies users: " + fleet.Count);
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Dailies (UTC) {DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm")}");
            var grouped = fleet.GroupBy(d => d.Squadron);
            foreach (var group in grouped)
            {
                message.AppendLine($"__{group.Key}__");
                foreach (var member in group)
                {
                    message.AppendLine($"{member.Name}: **{member.Dailies}**");
                }
                message.AppendLine(string.Empty);
            }

            HttpClient client = new HttpClient();
            var d1 = JsonConvert.SerializeObject(new { content = message.ToString() });
            var result = await client.PostAsync(configuration.DiscordConfiguration.DiscordDailiesWebhookUrl, new StringContent(d1, Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();

            return new OkResult();
        }
    }
}
