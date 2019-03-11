using System;
using System.Collections.Generic;
using System.Linq;

namespace Caliban.Core.World
{
    public class DesertNameGenerator
    {
        private List<string> claimedHashes = new List<string>();

        private static readonly string[] DesertNames = new string[2]
        {
            "sand",
            "dune"
        };

        private readonly Random random = new Random();

        public string GetRandomString(int _length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0987654321";
            string newHash = new string(Enumerable.Repeat(chars, _length)
                .Select(_s => _s[random.Next(_s.Length)]).ToArray());

            while (claimedHashes.Contains(newHash))
                newHash = new string(Enumerable.Repeat(chars, _length)
                    .Select(_s => _s[random.Next(_s.Length)]).ToArray());

            claimedHashes.Add(newHash);
            return newHash;
        }

        public string GetNewFolderName()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            var baseName = DesertNames[r.Next(DesertNames.Length)];
            var newFolderName = baseName + "_" + GetRandomString(8);

            return newFolderName;
        }
    }
}