using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SteamResume.Core;
using SteamResume.Models;
using SteamResume.Properties;
using SteamResume.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SteamResume.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region fields & constants

        private string apikey = "634272AA2FBE9BC5060D3B71EBEC1624";
        private DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private IPlayerRepository playerRepository;
        private HttpClient httpClient = new HttpClient();

        #endregion

        public MainViewModel(IPlayerRepository _playerRepository)
        {
            playerRepository = _playerRepository;

            // initialize commands
            CmdRefresh = new RelayCommand(CmdRefreshExecute);
            CmdSort = new RelayCommand(CmdSortExecute);
            CmdInitialize = new RelayCommand(CmdInitializeExecute);
        }

        #region properties

        private bool isUpdating;
        public bool IsUpdating
        {
            get { return isUpdating; }
            set { Set("IsUpdating", ref isUpdating, value); }
        }

        private Player player;
        public Player Player
        {
            get { return player; }
            set { Set("Player", ref player, value); }
        }

        private byte[] playerAvatar;
        public byte[] PlayerAvatar
        {
            get { return playerAvatar; }
            set { Set("PlayerAvatar", ref playerAvatar, value); }
        }
        
        private List<Game> completedApps = new List<Game>();
        public IEnumerable<Game> CompletedApps
        {
            get
            {
                string normalizedFilter = filter.ToLower();
                return completedApps?.Where(app => app.name.ToLower().Contains(normalizedFilter)
                                               || app.appid.Contains(normalizedFilter));
            }
        }
                
        private int counter1;
        public int Counter1
        {
            get { return counter1; }
            set { Set("Counter1", ref counter1, value); }
        }

        private int gameCount;
        public int GameCount
        {
            get { return gameCount; }
            set { Set("GameCount", ref gameCount, value); }
        }

        private int completedCount;
        public int CompletedCount
        {
            get { return completedCount; }
            set { Set("CompletedCount", ref completedCount, value); }
        }

        private int completedGameCount;
        public int CompletedGameCount
        {
            get { return completedGameCount; }
            set { Set("CompletedGameCount", ref completedGameCount, value); }
        }
        
        private int completedDemoCount;
        public int CompletedDemoCount
        {
            get { return completedDemoCount; }
            set { Set("CompletedDemoCount", ref completedDemoCount, value); }
        }

        private GroupCriteria groupCriteria = GroupCriteria.Date;
        public GroupCriteria GroupCriteria
        {
            get { return groupCriteria; }
            set
            {
                if (Set("GroupCriteria", ref groupCriteria, value))
                {
                    completedApps = OrderIntoGroup(completedApps, value).ToList();
                    RaisePropertyChanged("CompletedApps");
                }
            }
        }

        private string filter = string.Empty;
        public string Filter
        {
            get { return filter; }
            set
            {
                Set("Filter", ref filter, value);
                RaisePropertyChanged("CompletedApps");
            }
        }

        #endregion

        #region commands

        public RelayCommand CmdInitialize { get; private set; }
        public RelayCommand CmdRefresh { get; private set; }
        public RelayCommand CmdSort { get; private set; }
        
        #endregion

        #region command implementations

        private async void CmdInitializeExecute()
        {
            try
            {
                // try load from cache first
                if (playerRepository.HasCache(Settings.Default.steamid))
                {
                    var task1 = playerRepository.LoadPlayerAsync(Settings.Default.steamid);
                    //var task2 = playerRepository.LoadAvatarAsync(Settings.Default.steamid);
                    var task3 = playerRepository.LoadGamesAsync(Settings.Default.steamid);

                    Player = await task1;
                    //PlayerAvatar = await task2;
                    completedApps = OrderIntoGroup(await task3, GroupCriteria)?.ToList();

                    RaisePropertyChanged("CompletedApps");
                    CompletedCount = completedApps.Count();
                    CompletedGameCount = completedApps.Count(app => app.type == GameType.Game);
                    CompletedDemoCount = completedApps.Count(app => app.type == GameType.Demo);
                }
                else // refresh if no cache
                    CmdRefreshExecute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void CmdRefreshExecute()
        {
            IsUpdating = true;

            var engine = new SteamEngine(Settings.Default.steamid, apikey);
            DateTime now = DateTime.Now;

            // update player
            var taskUpdatePlayer = Task.Run(async () =>
            {
                Player = await engine.GetPlayerSummariesAsync();
                //PlayerAvatar = await httpClient.GetByteArrayAsync(Player.avatarfull);
                playerRepository.SavePlayerAsync(Settings.Default.steamid, Player);
                //playerRepository.SaveAvatarAsync(Settings.Default.steamid, PlayerAvatar);
            });

            // reset counters
            Counter1 = 0;
            GameCount = 0;
            
            // update owned apps
            ConcurrentBag<Game> completedOwnedGames = new ConcurrentBag<Game>();
            
            var taskUpdateOwnedApps = Task.Run(async () =>
            {
                List<Game> ownedApps = null;
                List<Game2> ownedAppsXml = null;
                
                // fetch app list
                var taskJson = Task.Run(async () => 
                {
                    ownedApps = (await engine.GetOwnedGamesAsync()).ToList();
                    GameCount = ownedApps.Count();
                });
                var taskXml = Task.Run(async () => 
                {
                    ownedAppsXml = (await engine.GetOwnedGames2Async()).Where(app => app.statsLink != null).ToList();
                });
                
                await Task.WhenAll(taskJson, taskXml);
                
                if (ownedApps == null || ownedAppsXml == null)
                    return;
                
                // fetch achievements for each app
                Parallel.ForEach(ownedApps, app =>
                {
                    Interlocked.Increment(ref counter1);
                    RaisePropertyChanged("Counter1");
                
                    if (!app.has_community_visible_stats)
                        return;
                
                    List<Achievement2> cheevos = null;
                
                    int retries = 1;
                    int retryCounter = 0;
                    bool apiCallSuccess = false;
                
                    while (!apiCallSuccess && ++retryCounter <= retries)
                    {
                        switch (app.appid)
                        {
                            case "630": // alien swarm
                                cheevos = engine.GetPlayerAchievementsAsync(app.appid).Result?.Select(a => new Achievement2 { closed = a.achieved }).ToList();
                                break;
                            default:
                                var appXml = ownedAppsXml.FirstOrDefault(aXml => aXml.appID == app.appid);
                                var appFriendlyName = appXml != null ? appXml.statsLink.Substring(appXml.statsLink.LastIndexOf('/') + 1) : null;
                                cheevos = engine.GetPlayerAchievements2Async(appFriendlyName != null ? appFriendlyName : app.appid).Result;
                                break;
                        }
                        
                        if (cheevos != null)
                            apiCallSuccess = true;
                    }
                                        
                    // really no stats, give up
                    if (cheevos == null || cheevos.Count == 0)
                        return;
                    
                    var cheevoTotalCount = cheevos.Count;
                    var cheevoUnlockedCount = cheevos.Count(c => c.closed > 0);
                
                    if (cheevoUnlockedCount == cheevoTotalCount)
                    {
                        // find last achie timestamp
                        var lastCheevo = cheevos.Aggregate((curMax, x) => (curMax == null || (x.unlockTimestamp == 0 ? int.MinValue : x.unlockTimestamp) > curMax.unlockTimestamp ? x : curMax));
                
                        var completedTimestamp = epoch.AddSeconds(lastCheevo.unlockTimestamp).ToLocalTime();
                        app.completed_timestamp = completedTimestamp;
                        app.type = GameType.Game;
                                                
                        completedOwnedGames.Add(app);
                    }
                });
            });

            // update extra apps (demos/unowned)
            ConcurrentBag<Game> completedUnownedApps = new ConcurrentBag<Game>();

            var taskUpdateExtraApps = Task.Run(async () => 
            {
                var extraApps = new List<Game>();
                var extraFile = "list_extras.txt";
                if (File.Exists(extraFile))
                {
                    foreach (var line in File.ReadAllLines(extraFile))
                    {
                        var split = line.Split(',');
                        if (split.Length == 3)
                            extraApps.Add(new Game
                            {
                                appid = split[0],
                                type = (GameType)Enum.Parse(typeof(GameType), split[1], true),
                                name = split[2]
                            });
                    }
                }
                else
                {
                    var storeApps = await engine.GetAppListAsync();
                    // demos & udk
                    extraApps = storeApps.Where(app => app.name.ToLower().EndsWith("demo") || app.appid == "13260").ToList();
                }

                Parallel.ForEach(extraApps, app => 
                {
                    var cheevos = engine.GetPlayerAchievements2Async(app.appid).Result;
                    
                    if (cheevos == null)
                        cheevos = engine.GetPlayerAchievementsAsync(app.appid).Result?.Select(a => new Achievement2 { closed = a.achieved }).ToList();

                    if (cheevos == null || cheevos.Count == 0)
                        return;
                    
                    var cheevoTotalCount = cheevos.Count;
                    var cheevoUnlockedCount = cheevos.Count(c => c.closed > 0);
                    
                    if (cheevoUnlockedCount == cheevoTotalCount)
                    {
                        // find last achie timestamp
                        var lastCheevo = cheevos.Aggregate((curMax, x) => (curMax == null || (x.unlockTimestamp == 0 ? int.MinValue : x.unlockTimestamp) > curMax.unlockTimestamp ? x : curMax));
                    
                        var completedTimestamp = epoch.AddSeconds(lastCheevo.unlockTimestamp).ToLocalTime();
                        app.completed_timestamp = completedTimestamp;
                                        
                        completedUnownedApps.Add(app);
                    }
                });
            });

            await Task.WhenAll(taskUpdatePlayer, taskUpdateOwnedApps, taskUpdateExtraApps);

            completedApps = OrderIntoGroup(completedOwnedGames.Union(completedUnownedApps), GroupCriteria).ToList();
            CompletedCount = completedApps.Count();
            CompletedGameCount = completedApps.Count(a => a.type == GameType.Game);
            CompletedDemoCount = completedApps.Count(a => a.type == GameType.Demo);

            RaisePropertyChanged("CompletedApps");
            
            DateTime then = DateTime.Now;
            Console.WriteLine("time: {0} s", then.Subtract(now).TotalSeconds);
            IsUpdating = false;

            // persist
            await playerRepository.SaveGamesAsync(Player.steamid, completedApps);
        }

        private void CmdSortExecute()
        {
            GroupCriteria = 1 - GroupCriteria;
        }

        #endregion

        #region private methods
        
        private IEnumerable<Game> OrderIntoGroup(IEnumerable<Game> games, GroupCriteria criteria)
        {
            if (games == null)
                return null;

            switch (criteria)
            {
                case GroupCriteria.Date:
                    foreach (var a in games)
                        a.group = new DateTime(a.completed_timestamp.Year, a.completed_timestamp.Month, 1).ToString("MMM yyyy"); ;
                    return games.OrderByDescending(g => g.completed_timestamp).ThenBy(g => g.name.ToLower());
                case GroupCriteria.Name:
                    foreach (var a in games)
                        a.group = Char.IsDigit(a.name[0]) ? "#" : a.name[0].ToString().ToUpper();
                    return games.OrderBy(g => g.group).ThenBy(g => g.name);
            }

            return null;
        }

        #endregion
    }
}
