using System.Security.Cryptography;
using System.Text;
using SeedWave.Core.Catalog;

namespace SeedWave.Core.Generation
{
    /// <summary>
    /// Uses a stable hash of song identity and purpose to derive deterministic generator seeds.
    /// </summary>
    public class SeedDeriver : ISeedDeriver
    {
        public int Derive(SongIdentity identity, SeedPurpose purpose)
        {
            string input = 
                $"{identity.Region}|{identity.UserSeed}|{identity.Page}|{identity.SequenceIndex}|{(int)purpose}";

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

            return BitConverter.ToInt32(hash, 0);
        }
    }
}