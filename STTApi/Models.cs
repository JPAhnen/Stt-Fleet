using System.Collections.Generic;

namespace STTFleet.STTApi 
{
    public class Chatchannels
   {
      public string a { get; set; }
      public string o { get; set; }
   }

   public class Squad
   {
      public object id { get; set; }
      public string nleader_player_dbid { get; set; }
      public string slabel { get; set; }
      public string name { get; set; }
      public string enrollment { get; set; }
      public string description { get; set; }
      public string motd { get; set; }
      public Chatchannels chatchannels { get; set; }
      public int maxsize { get; set; }
      public int cursize { get; set; }
      public object created { get; set; }
      public object rootguild { get; set; }
      public int? event_rank { get; set; }
   }

   public class Icon
   {
      public string file { get; set; }
   }

   public class Portrait
   {
      public string file { get; set; }
   }

   public class FullBody
   {
      public string file { get; set; }
   }

   public class CrewAvatar
   {
      public int id { get; set; }
      public string symbol { get; set; }
      public string name { get; set; }
      public List<string> traits { get; set; }
      public List<string> traits_hidden { get; set; }
      public string short_name { get; set; }
      public int max_rarity { get; set; }
      public Icon icon { get; set; }
      public Portrait portrait { get; set; }
      public FullBody full_body { get; set; }
      public bool default_avatar { get; set; }
      public bool hide_from_cryo { get; set; }
      public List<string> skills { get; set; }
   }

   public class Member
   {
      public object dbid { get; set; }
      public object squad_id { get; set; }
      public string display_name { get; set; }
      public int level { get; set; }
      public CrewAvatar crew_avatar { get; set; }
      public int pid { get; set; }
      public int uid { get; set; }
      public string rank { get; set; }
      public string squad_rank { get; set; }
      public int last_active { get; set; }
      public int starbase_activity { get; set; }
      public int daily_activity { get; set; }
      public int? event_rank { get; set; }
   }

   public class FleetMemberInfo
   {
      public string action { get; set; }
      public List<Squad> squads { get; set; }
      public List<Member> members { get; set; }
   }
}