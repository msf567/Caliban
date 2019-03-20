using System;
using System.Collections.Generic;
using System.IO;
using Caliban.Core.Treasures;
using Caliban.Core.Utility;
using Caliban.Core.World;

namespace Caliban.Core.Game
{
    public class MapClue : Clue
    {
        Random r = new Random(Guid.NewGuid().GetHashCode());

        public MapClue(string _clueLocation, World.World _d, int _spawnDepth = 2) : base(_clueLocation)
        {
            var spawnNode = _d.WorldRoot.GetAllNodesAtDepth(_spawnDepth);
            int random = r.Next(0, spawnNode.Count);
            var mapLocation = spawnNode[random].FullName;
            string trimmedLoc =mapLocation.Replace(WorldParameters.WorldRoot.FullName, "");
            ClueManager.AddMapLocation(trimmedLoc, clueLocation.FullName);
            spawnNode[random].AddTreasure(TreasureType.TORN_MAP,"TornMap.exe");
            D.Write("Spawning map in " + trimmedLoc);
        }

        public override void Dispose()
        {
        }
    }
}