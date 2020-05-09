using System;
using System.Collections.Generic;

namespace STTFleet
{
    public class FleetData 
    {
        public string id { get; set; }
        public string Ident {get; set;}
        public List<SquadHistory> Squads {get; set;}
        public List<PlayerHistory> Players {get; set;}
    }

    public class SquadHistory
    {
        public string Name {get; set;}
        public string Id {get; set;}
        public Dictionary<DateTime, EventRank> EventRanks {get; set;}
        public EventRank LastEventRank {get; set;}
    }

    public class PlayerHistory
    {
        public string Name {get; set;}
        public string Id {get; set;}
        public string SquadId {get; set;}
        public Dictionary<DateTime, EventRank> EventRanks {get; set;}
        public EventRank LastEventRank {get; set;}
    }

    public class EventRank
    {
        public DateTime DateTime { get; set; }
        public int Rank {get; set;}
    }
}