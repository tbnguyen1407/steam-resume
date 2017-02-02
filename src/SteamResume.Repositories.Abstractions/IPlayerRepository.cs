using SteamResume.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamResume.Repositories
{
    public interface IPlayerRepository
    {
        Task<byte[]> LoadAvatarAsync(string playerid);
        Task<IEnumerable<Game>> LoadGamesAsync(string playerid);
        Task<Player> LoadPlayerAsync(string playerid);
        Task<bool> SaveAvatarAsync(string playerid, byte[] bytes);
        Task<bool> SaveGamesAsync(string playerid, IEnumerable<Game> games);
        Task<bool> SavePlayerAsync(string playerid, Player player);
        bool HasCache(string playerid);
    }
}
