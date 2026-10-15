using System;
using Caliban.Core.World;
using System.Text.Json;

namespace Caliban.Core.Transport
{
    [Serializable]
    public class GraphicsSceneDescription
    {
        // The scene (and the Feature hierarchy) use public fields rather than properties, so
        // System.Text.Json must be told to include fields; otherwise the parameterized
        // constructor's parameters cannot bind to the Seed/Features fields on deserialization.
        // This single shared options instance is used on both the serialize and deserialize sides.
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true
        };

        public GraphicsSceneDescription(int seed, Feature[] features)
        {
            Seed = seed;
            Features = features;
        }

        public int Seed;
        public Feature[] Features;

        public byte[] GetBytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this, JsonOptions);
        }
    }
}