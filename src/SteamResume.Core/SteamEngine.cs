using Newtonsoft.Json;
using SteamResume.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SteamResume.Core
{
    public class SteamEngine
    {
        private HttpClient httpClient = new HttpClient();

        private string steamID64;
        private string apiKey;
        private const string phInterface = "[interface]";
        private const string phMethod = "[method]";
        private const string phVersion = "[version]";
        private string apiTemplate = @"http://api.steampowered.com/[interface]/[method]/[version]/";

        public SteamEngine(string _steamid, string _apikey)
        {
            steamID64 = _steamid;
            apiKey = _apikey;
        }

        #region web apis

        public async Task<string> ResolveVanityUrlAsync(string steamVanityUrl)
        {
            string url = apiTemplate
                .Replace(phInterface, "ISteamUser")
                .Replace(phMethod, "ResolveVanityUrl")
                .Replace(phVersion, "v0001");
            url += string.Format("?key={0}&vanityurl={1}", apiKey, steamVanityUrl);

            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<ResolveVanityUrlResult>(stringResult);
                return result.response.steamid;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Player> GetPlayerSummariesAsync()
        {
            string url = apiTemplate
                .Replace(phInterface, "ISteamUser")
                .Replace(phMethod, "GetPlayerSummaries")
                .Replace(phVersion, "v0002");
            url += string.Format("?key={0}&steamids={1}", apiKey, steamID64);

            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<GetPlayerSummariesResult>(stringResult);
                return result.response.players.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<int> GetSteamLevelAsync()
        {
            string url = apiTemplate
                .Replace(phInterface, "IPlayerService")
                .Replace(phMethod, "GetSteamLevel")
                .Replace(phVersion, "v0001");
            url += string.Format("?key={0}&steamid={1}", apiKey, steamID64);

            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<GetSteamLevelResult>(stringResult);
                return result.response.player_level;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public async Task<IEnumerable<Game>> GetAppListAsync()
        {
            string url = apiTemplate
                .Replace(phInterface, "ISteamApps")
                .Replace(phMethod, "GetAppList")
                .Replace(phVersion, "v0002");
            
            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<GetAppListResult>(stringResult);
                return result.applist.apps;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<Game>> GetOwnedGamesAsync()
        {
            string url = apiTemplate
                .Replace(phInterface, "IPlayerService")
                .Replace(phMethod, "GetOwnedGames")
                .Replace(phVersion, "v0001");
            url += string.Format("?key={0}&steamid={1}&include_appinfo={2}&include_played_free_games={3}", apiKey, steamID64, 1, 1);

            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<GetOwnedGamesResult>(stringResult);
                return result.response.games;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
                
        public async Task<IEnumerable<Game2>> GetOwnedGames2Async()
        {
            string url = string.Format("http://steamcommunity.com/profiles/{0}/games/?tab=all&sort=name&xml=1", steamID64);

            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);

                var serializer = new XmlSerializer(typeof(gamesList));
                var result = (gamesList)serializer.Deserialize(new StringReader(stringResult));

                return result.games;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<List<Achievement>> GetPlayerAchievementsAsync(string appid)
        {
            string url = apiTemplate
                .Replace(phInterface, "ISteamUserStats")
                .Replace(phMethod, "GetPlayerAchievements")
                .Replace(phVersion, "v0001");
            url += string.Format("?key={0}&steamid={1}&appid={2}", apiKey, steamID64, appid);

            try
            {
                var stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<GetPlayerAchievementsResult>(stringResult);
                return result.playerstats.achievements.ToList();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Achievement2>> GetPlayerAchievements2Async(string appid)
        {
            string url = string.Format("http://steamcommunity.com/profiles/{0}/stats/{1}/achievements/?xml=1", steamID64, appid);
            string stringResult = null;

            try
            {
                stringResult = await httpClient.GetStringAsync(url).ConfigureAwait(false);

                // result in html instead of xml
                if (!stringResult.StartsWith("<?xml"))
                    return null;

                var serializer = new XmlSerializer(typeof(playerstats));
                var result = (playerstats)serializer.Deserialize(new StringReader(stringResult));
                
                return result.achievements;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(stringResult))
                    File.WriteAllText($"db/{appid}.log", stringResult);
                return null;
            }
        }

        #endregion

        private bool IsSteamID64(string input)
        {
            return true;
        }
    }

    #region json results

    public class ResolveVanityUrlResult
    {
        public ResolveVanityUrlResponse response { get; set; }
    }

    public class ResolveVanityUrlResponse
    {
        public string steamid { get; set; }
        public int success { get; set; } // 1 success, 42 no match
        public string message { get; set; }
    }

    public class GetPlayerSummariesResult
    {
        public GetPlayerSummariesRoot response { get; set; }
    }

    public class GetSteamLevelResult
    {
        public GetSteamLevelRoot response { get; set; }
    }

    public class GetSteamLevelRoot
    {
        public int player_level { get; set; }
    }

    public class GetPlayerSummariesRoot
    {
        public IEnumerable<Player> players { get; set; }
    }
        
    public class GetPlayerAchievementsResult
    {
        public GetPlayerAchievementsRoot playerstats { get; set; }
    }

    public class GetPlayerAchievementsRoot
    {
        public string steamID { get; set; }
        public string gamename { get; set; }
        public IEnumerable<Achievement> achievements { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
    }

    public class GetAppListResult
    {
        public GetAppListResponse applist { get; set; }
    }

    public class GetAppListResponse
    {
        public IEnumerable<Game> apps { get; set; }
    }

    public class GetOwnedGamesResult
    {
        public GetOwnedGamesRoot response { get; set; }
    }

    public class GetOwnedGamesRoot
    {
        public int game_count { get; set; }
        public IEnumerable<Game> games { get; set; }
    }

    #endregion

    #region xml results

    public class gamesList
    {
        public string steamID64 { get; set; }
        public string steamID { get; set; }
        [XmlArrayItem("game")]
        public List<Game2> games { get; set; }
    }

    public class playerstats
    {
        [XmlArrayItem("achievement")]
        public List<Achievement2> achievements { get; set; }
    }

    #endregion
}
