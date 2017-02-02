using System;
using System.Collections.Generic;
using SteamResume.Models;
using System.IO;
using Newtonsoft.Json;
using SteamResume.Repositories.Helpers;
using System.Threading.Tasks;

namespace SteamResume.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private static string dbRoot = "db";
        
        public async Task<byte[]> LoadAvatarAsync(string playerid)
        {
            string playerFolder = $"{dbRoot}/{playerid}";
            
            try
            {
                var bytes = await FileHelper.ReadBytesAsync($"{playerFolder}/avatar.jpg");
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<IEnumerable<Game>> LoadGamesAsync(string playerid)
        {
            string playerFolder = $"{dbRoot}/{playerid}";

            try
            {
                var fileContent = await FileHelper.ReadTextAsync($"{playerFolder}/games.txt");
                return JsonConvert.DeserializeObject<List<Game>>(fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<Player> LoadPlayerAsync(string playerid)
        {
            string playerFolder = $"{dbRoot}/{playerid}";

            try
            {
                var fileContent = await FileHelper.ReadTextAsync($"{playerFolder}/player.txt");
                return JsonConvert.DeserializeObject<Player>(fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<bool> SaveAvatarAsync(string playerid, byte[] bytes)
        {
            string playerFolder = $"{dbRoot}/{playerid}";

            try
            {
                if (!Directory.Exists(playerFolder))
                    Directory.CreateDirectory(playerFolder);

                await FileHelper.WriteBytesAsync($"{playerFolder}/avatar.jpg", bytes);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> SaveGamesAsync(string playerid, IEnumerable<Game> games)
        {
            string playerFolder = $"{dbRoot}/{playerid}";

            try
            {
                if (!Directory.Exists(playerFolder))
                    Directory.CreateDirectory(playerFolder);

                var fileContent = JsonConvert.SerializeObject(games, Formatting.Indented);
                await FileHelper.WriteTextAsync($"{playerFolder}/games.txt", fileContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> SavePlayerAsync(string playerid, Player player)
        {
            string playerFolder = $"{dbRoot}/{playerid}";

            try
            {
                if (!Directory.Exists(playerFolder))
                    Directory.CreateDirectory(playerFolder);

                var fileContent = JsonConvert.SerializeObject(player, Formatting.Indented);
                await FileHelper.WriteTextAsync($"{playerFolder}/player.txt", fileContent);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool HasCache(string playerid)
        {
            string playerFolder = $"{dbRoot}/{playerid}";
            return File.Exists($"{playerFolder}/player.txt")
                //&& File.Exists($"{playerFolder}/avatar.jpg")
                && File.Exists($"{playerFolder}/games.txt");
        }
    }
}
