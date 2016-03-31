using Sodes.Bridge.Base;
using System.IO;
using System.Threading.Tasks;

namespace Sodes.Bridge.Base
{
    public static class TournamentLoader
    {
        /// <summary>
        /// Read a pbn file
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static async Task<Tournament> LoadAsync(Stream fileStream)
        {
            using (var sr = new StreamReader(fileStream))
            {
                string content = await sr.ReadToEndAsync();
                return Pbn2Tournament.Load(content);
            }
        }

        public static void Save(Stream fileStream, Tournament tournament)
        {
            Pbn2Tournament.Save(tournament, fileStream);
        }
    }
}
