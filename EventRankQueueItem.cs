using System.Collections.Generic;
using STTFleet.STTApi;

namespace STTFleet
{
    public class EventRankQueueItem
    {
        public List<UserDailies> UserDailies { get; set; }
        public List<SquadEventRank> SquadEventRanks { get; set; }
    }
}