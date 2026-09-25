using System;
using Caliban.Core.World;
using System.Text.Json;

namespace Caliban.Core.Transport
{
    [Serializable]
    public class GraphicsSceneDescription
    {
        public GraphicsSceneDescription(int seed, Feature[] features)
        {
            Seed = seed;
            Features = features;
        }

        public int Seed;
        public Feature[] Features;

        public byte[] GetBytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }
    }
}