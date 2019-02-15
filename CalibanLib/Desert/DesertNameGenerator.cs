using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZetaLongPaths;

namespace CalibanLib.Desert
{
    public class DesertNameGenerator
    {
        private List<string> claimedHashes = new List<string>();
        private static readonly string[] DesertNames = new string[2]
        {
            "sand",
            "dune"
        };
        
        private  readonly Random random = new Random();
        public  string GetRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0987654321";
            string newHash = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            while(claimedHashes.Contains(newHash))
                newHash = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            
            claimedHashes.Add(newHash);
            return newHash;
        }
        public  string GetNewFolderName(ZlpDirectoryInfo parent)
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            var baseName = DesertNames[r.Next(DesertNames.Length)];
            var newFolderName = baseName +"_"+ GetRandomString(8);

            return newFolderName;
        }
    }
}