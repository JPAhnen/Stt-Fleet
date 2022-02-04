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
using System.Text.RegularExpressions;

namespace STTFleet
{
    public static class FleetDailies
    {
        [FunctionName("FleetDailiesToDiscord")]
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
            string now = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm");
            var grouped = fleet.GroupBy(d => d.Squadron);

            StringBuilder message1 = new StringBuilder();
            message1.AppendLine($"(1) Dailies (UTC) {now}");
            foreach (var group in grouped.Take(5))
            {
                var squadName = Regex.Replace(group?.Key ?? string.Empty, @"\<\#[^<>]*\>", string.Empty);
                message1.AppendLine($"__{squadName}__");
                foreach (var member in group)
                {
                    message1.AppendLine($"{member.Name}: **{member.Dailies}** -- **{member.DailyMissions}**");
                }
                message1.AppendLine(string.Empty);
            }

            StringBuilder message2 = new StringBuilder();
            message2.AppendLine("        -");
            foreach (var group in grouped.Skip(5).Take(5))
            {
                var squadName = Regex.Replace(group?.Key ?? string.Empty, @"\<\#[^<>]*\>", string.Empty);
                message2.AppendLine($"__{squadName}__");
                foreach (var member in group)
                {
                    message2.AppendLine($"{member.Name}: **{member.Dailies}** -- **{member.DailyMissions}**");
                }
                message2.AppendLine(string.Empty);
            }

            HttpClient client = new HttpClient();
            var d1 = JsonConvert.SerializeObject(new { content = message1.ToString() });
            var result1 = await client.PostAsync(configuration.DiscordConfiguration.DiscordDailiesWebhookUrl, new StringContent(d1, Encoding.UTF8, "application/json"));
            result1.EnsureSuccessStatusCode();

            if (grouped.Count() > 5)
            {
                var d2 = JsonConvert.SerializeObject(new { content = message2.ToString() });
                var result2 = await client.PostAsync(configuration.DiscordConfiguration.DiscordDailiesWebhookUrl, new StringContent(d2, Encoding.UTF8, "application/json"));
                result2.EnsureSuccessStatusCode();
            }
            return new OkResult();
        }
    }
}
