using System.Xml.Serialization;

namespace SteamResume.Models
{
    public class Achievement
    {
        public string apiname { get; set; }
        public int achieved { get; set; }
    }

    public class Achievement2
    {
        [XmlAttribute]
        public int closed { get; set; }
        public string iconClosed { get; set; }
        public string iconOpen { get; set; }
        public string name { get; set; }
        public string apiname { get; set; }
        public string description { get; set; }
        public int unlockTimestamp { get; set; }
    }
}
