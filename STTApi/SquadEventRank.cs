using System.Collections.Generic;
using System.Linq;

namespace STTFleet.STTApi
{
    public class SquadEventRank
    {
        public string Name { get; set; }
        public string SquadronId { get; set; }
        public int EventRank { get; set; }
        
        public static List<SquadEventRank> Load(FleetMemberInfo memberInfo)
        {
            var squads = new List<SquadEventRank>();
            
            squads.AddRange(memberInfo.squads.Select(s => new SquadEventRank() { Name = s.name, SquadronId = s.id?.ToString(), EventRank = s.event_rank ?? 0 }));

            return squads;
        }
    }
}