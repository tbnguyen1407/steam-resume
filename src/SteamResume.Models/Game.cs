using System;

namespace SteamResume.Models
{
    public class Game
    {
        public string appid { get; set; }
        public string name { get; set; }
        //public int playtime_2weeks { get; set; }
        //public int playtime_forever { get; set; }
        //public string img_icon_url { get; set; }
        //public string img_logo_url { get; set; }
        public bool has_community_visible_stats { get; set; }

        // added
        public string friendlyname { get; set; } // to get from xml game details
        public GameType type { get; set; }
        public DateTime completed_timestamp { get; set; }
        public string group { get; set; }
    }

    public class Game2
    {
        public string appID { get; set; }
        public string name { get; set; }
        //public string logo { get; set; }
        //public string storeLink { get; set; }
        //public double hoursLast2Weeks { get; set; }
        //public double hoursOnRecord { get; set; }
        public string statsLink { get; set; }
        //public string globalStatsLink { get; set; }
    }

    public enum GameType
    {
        App,
        Demo,
        Game
    }
}
