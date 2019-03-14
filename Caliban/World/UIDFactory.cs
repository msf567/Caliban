using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace Caliban.Core.World
{
    public static class UIDFactory
    {      
        private static readonly Random random = new Random();
        private static string Chars = "abcdefghijklmnopqrstuvwxyz0987654321";

        public static string GetNewUID(int _length, List<string> _collection)
        {
            var newUID = new string(Enumerable.Repeat(Chars, _length)
                .Select(_s => _s[random.Next(_s.Length)]).ToArray());

            while (_collection.Contains(newUID))
                newUID = new string(Enumerable.Repeat(Chars, _length)
                    .Select(_s => _s[random.Next(_s.Length)]).ToArray());

            _collection.Add(newUID);
            return newUID;
        }
    }
}