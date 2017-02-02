namespace SteamResume.Models
{
    public class Player
    {
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string avatar { get; set; }
        public string avatarfull { get; set; }
        public string avatarmedium { get; set; }
        public string lastlogoff { get; set; }
        public string loccountrycode { get; set; }
        public string personaname { get; set; }
        public string personastate { get; set; }
        public string personastateflags { get; set; }
        public string primaryclanid { get; set; }
        public string profileurl { get; set; }
        public string realname { get; set; }
        public string steamid { get; set; }
        public string timecreated { get; set; }
    }

    public class Player2
    {
        public string steamID64 { get; set; }
        public string customURL { get; set; }
    }
}
