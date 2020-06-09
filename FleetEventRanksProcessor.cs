using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using STTFleet.STTApi;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace STTFleet
{
    public static class FleetEventRanksProcessor
    {
        [FunctionName("FleetEventRanksProcessor")]
        public static void Run(
            [QueueTrigger("fleeteventranks", Connection = "sttfleetg5618_STORAGE")] EventRankQueueItem queueItem,
            [CosmosDB(
                databaseName: "g5618-fleet",
                collectionName: "fleetdata",
                ConnectionStringSetting = "sttfleetg5618_COSMOSDB")]IEnumerable<FleetData> fleetDataList,
            [CosmosDB(
                databaseName: "g5618-fleet",
                collectionName: "fleetdata",
                ConnectionStringSetting = "sttfleetg5618_COSMOSDB")]out FleetData updatedFleetData,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");
            var configuration = new Configuration(context.FunctionAppDirectory);

            FleetData fleetData = fleetDataList.FirstOrDefault(f => f.Ident == "current");
            if (fleetData == null)
            {
                fleetData = new FleetData() { Ident = "current", Squads = new List<SquadHistory>() };
            }

            // Squads
            fleetData.Squads = _UpdateSquadHistory(fleetData, queueItem.SquadEventRanks);
            fleetData.Players = _UpdatePlayerHistory(fleetData, queueItem.UserDailies);
            updatedFleetData = fleetData;

            // try
            // {
            //     var t = _SendToDiscord(log, queueItem.UserDailies, fleetData, configuration.DiscordConfiguration.DiscordEventRanksWebhookUrl);
            //     Task.WaitAll(t);
            // }
            // catch (Exception e)
            // {
            //     log.LogError($"Send to Discord failed: {e.Message}");
            // }
        }

        private static async Task _SendToDiscord(ILogger log, List<UserDailies> players, FleetData fleetData, string url)
        {
            string header = $"Events (UTC) {DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm")}";
            var grouped = players.GroupBy(d => d.SquadronId);

            var g1 = grouped.Take(5);
            var g2 = grouped.Skip(5);
            await _PostGroupToDiscord(g1, fleetData, url, $"{header}");
            await _PostGroupToDiscord(g2, fleetData, url, $"        -");
        }

        private static async Task _PostGroupToDiscord(IEnumerable<IGrouping<string, UserDailies>> grouped, FleetData fleetData, string url, string header)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(header);

            foreach (var group in grouped)
            {
                var squad = fleetData.Squads.FirstOrDefault(s => s.Id == group.Key);

                message.AppendLine($"__{squad?.Name ?? "Ohne Squad"}__ --- **{squad?.LastEventRank.Rank ?? 0}** --- (Avg {squad?.EventRanks.Select(e => e.Value.Rank).Average() ?? 0} of {squad?.EventRanks.Count ?? 0})");
                foreach (var member in group)
                {
                    var player = fleetData.Players.FirstOrDefault(p => p.Id == member.UserId);
                    if (player != null)
                    {
                        int avg = (int)Math.Floor(player.EventRanks.Select(e => e.Value.Rank).Average());

                        message.AppendLine($"{player.Name}: **{player.LastEventRank.Rank}** --- (Avg {avg} of {player.EventRanks.Count})");
                    }
                }
                message.AppendLine(string.Empty);
            }

            HttpClient client = new HttpClient();
            var d1 = JsonConvert.SerializeObject(new { content = message.ToString() });
            var result = await client.PostAsync(url, new StringContent(d1, Encoding.UTF8, "application/json"));

            result.EnsureSuccessStatusCode();
        }

        private static List<SquadHistory> _UpdateSquadHistory(FleetData fleetData, List<SquadEventRank> squadEventRanks)
        {
            var updatedSquadsLocal = new List<SquadHistory>();
            if (squadEventRanks != null)
            {
                squadEventRanks.ForEach(squad =>
                {
                    var s = fleetData?.Squads?.FirstOrDefault(i => i.Id == squad.SquadronId) ?? new SquadHistory() { Id = squad.SquadronId, EventRanks = new Dictionary<DateTime, EventRank>() };
                    s.Name = squad.Name;
                    s.LastEventRank = new EventRank() { DateTime = DateTime.UtcNow.Date, Rank = squad.EventRank };

                    if (squad.EventRank > 0)
                    {
                        s.EventRanks[s.LastEventRank.DateTime] = s.LastEventRank;
                    }
                    updatedSquadsLocal.Add(s);
                });
            }

            return updatedSquadsLocal;
        }

        private static List<PlayerHistory> _UpdatePlayerHistory(FleetData fleetData, List<UserDailies> playerEventRanks)
        {
            var updatedPlayersLocal = new List<PlayerHistory>();
            if (playerEventRanks != null)
            {
                playerEventRanks.ForEach(player =>
                {
                    var s = fleetData?.Players?.FirstOrDefault(i => i.Id == player.UserId) ?? new PlayerHistory() { Id = player.UserId, EventRanks = new Dictionary<DateTime, EventRank>() };
                    s.Name = player.Name;
                    s.SquadId = player.SquadronId;
                    s.LastEventRank = new EventRank() { DateTime = DateTime.UtcNow.Date, Rank = player.EventRank ?? 0 };
                    if (player.EventRank > 0)
                    {
                        s.EventRanks[s.LastEventRank.DateTime] = s.LastEventRank;
                    }
                    updatedPlayersLocal.Add(s);
                });
            }

            return updatedPlayersLocal;
        }
    }
}
