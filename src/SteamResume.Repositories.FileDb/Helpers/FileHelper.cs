using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteamResume.Repositories.Helpers
{
    class FileHelper
    {
        public static async Task WriteBytesAsync(string filePath, byte[] content)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 4096, true))
            {
                fs.Seek(0, SeekOrigin.Begin);
                await fs.WriteAsync(content, 0, content.Length);
            }
        }

        public static async Task WriteTextAsync(string filePath, string content)
        {
            byte[] encodedContent = Encoding.UTF8.GetBytes(content);

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 4096, true))
                await fs.WriteAsync(encodedContent, 0, encodedContent.Length);
        }


        public static async Task<byte[]> ReadBytesAsync(string filePath)
        {
            byte[] bytes;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                bytes = new byte[fs.Length];
                await fs.ReadAsync(bytes, 0, (int)fs.Length);
            }
            return bytes;
        }
        public static async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, numRead));

                return sb.ToString();
            }
        }
    }
}
