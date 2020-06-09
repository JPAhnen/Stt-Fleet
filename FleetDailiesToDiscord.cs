using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using STTFleet.STTApi;

namespace STTFleet
{
    public static class FleetDailiesToDiscord
    {
        [FunctionName("FleetDailiesToDiscord")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "g5618-fleet",
            collectionName: "fleetdata",
            ConnectionStringSetting = "sttfleetg5618_COSMOSDB",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionName = "leases")]IReadOnlyList<Document> document, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# CosmosDB trigger function");

            var configuration = new Configuration(context.FunctionAppDirectory);

            var fleetData = JsonConvert.DeserializeObject<FleetData>(document.First().ToString());
            
            try
            {
                var t = _SendToDiscord(log, fleetData.Players, fleetData, configuration.DiscordConfiguration.DiscordEventRanksWebhookUrl);
                Task.WaitAll(t);
            }
            catch (Exception e)
            {
                log.LogError($"Send to Discord failed: {e.Message}");
            }
        }

        private static async Task _SendToDiscord(ILogger log, List<PlayerHistory> players, FleetData fleetData, string url)
        {
            string header = $"Events (UTC) {DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm")}";
            var grouped = players.GroupBy(d => d.SquadId);

            var g1 = grouped.Take(5);
            var g2 = grouped.Skip(5);
            await _PostGroupToDiscord(g1, fleetData, url, $"{header}");
            await _PostGroupToDiscord(g2, fleetData, url, $"        -");
        }

        private static async Task _PostGroupToDiscord(IEnumerable<IGrouping<string, PlayerHistory>> grouped, FleetData fleetData, string url, string header)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(header);

            foreach (var group in grouped)
            {
                var squad = fleetData.Squads.FirstOrDefault(s => s.Id == group.Key);

                message.AppendLine($"__{squad?.Name ?? "Ohne Squad"}__ --- **{squad?.LastEventRank.Rank ?? 0}** --- (Avg {squad?.EventRanks.Select(e => e.Value.Rank).Average() ?? 0} of {squad?.EventRanks.Count ?? 0})");
                foreach (var member in group)
                {
                    var player = fleetData.Players.FirstOrDefault(p => p.Id == member.Id);
                    int avg = (int)Math.Floor(player?.EventRanks.Select(e => e.Value.Rank).Average() ?? 0);

                    message.AppendLine($"{member.Name}: **{player?.LastEventRank.Rank ?? 0}** --- (Avg {avg} of {player?.EventRanks.Count ?? 0})");
                }
                message.AppendLine(string.Empty);
            }

            HttpClient client = new HttpClient();
            var d1 = JsonConvert.SerializeObject(new { content = message.ToString() });
            var result = await client.PostAsync(url, new StringContent(d1, Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();
        }
    }
}
