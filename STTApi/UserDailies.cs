using System.Collections.Generic;
using System.Linq;

namespace STTFleet.STTApi
{
    public class UserDailies
   {
      public string Name { get; set; }
      public string UserId { get; set; }
      public int Dailies { get; set; }
      public string Squadron { get; set; }
      public string SquadronId { get; set; }
      public int? EventRank { get; set; }
      
      public static List<UserDailies> Load(FleetMemberInfo memberInfo)
      {
         var squads = memberInfo.squads.ToDictionary(s => s.id.ToString(), s => s.name);

         var dailiesPerMember = memberInfo.members.Select(member => new UserDailies 
         { 
            Name = member.display_name, 
            UserId = member.dbid.ToString(),
            Dailies = member.daily_activity, 
            Squadron = _GetSquadName(member.squad_id.ToString(), squads),
            SquadronId = member.squad_id.ToString(),
            EventRank = member.event_rank }).ToList();
         return dailiesPerMember.OrderByDescending(d => d.Squadron).ThenByDescending(d => d.Name).ToList();
      }

      private static string _GetSquadName(string id, IDictionary<string, string> squads)
      {
         bool found = squads.TryGetValue(id, out string name);

         if (!found) return "Ohne Squad";

         return name;
      }
   }
}