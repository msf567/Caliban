using System;
using System.Collections.Generic;
using Caliban.Core.Utility;
using Caliban.Core.World;
using Treasures.Resources;
using Caliban.Core.Debug;

namespace Caliban.Core.Game
{
    public class MapClue : Clue
    {
        Random r = new Random(Guid.NewGuid().GetHashCode());

        public MapClue(string _locationToPointTo, World.World _d, int _spawnDepth = 2) : base(_locationToPointTo)
        {
            var spawnNode = _d.WorldRoot.GetAllNodesAtDepth(_spawnDepth);
            int random = r.Next(0, spawnNode.Count);
            var mapLocation = spawnNode[random].FullName.Replace(WorldParameters.WorldRoot.FullName, "");
            D.Write("Spawning map in " + mapLocation);
            spawnNode[random].AddTreasure(TreasureType.TORN_MAP, "Caliban.TornMap.exe", new Dictionary<string, string>
            {
                { "location", locationToPointTo.FullName }
            });
        }

        public override void Dispose()
        {
        }
    }
}