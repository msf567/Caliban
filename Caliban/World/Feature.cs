using System;
using System.Text.Json.Serialization;

namespace Caliban.Core.World
{
    public enum FeatureType
    {
        CACTUS,
        ROCK,
    }

    // Feature is abstract, so System.Text.Json needs polymorphic type info to know which
    // concrete type to instantiate when deserializing a Feature[] (e.g. in GraphicsSceneDescription).
    [Serializable]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(CactusFeature), "cactus")]
    [JsonDerivedType(typeof(RockFeature), "rock")]
    public abstract class Feature
    {
        public FeatureType Type;
        public double X;
        public double Y;
    }

    public class CactusFeature : Feature
    {
        public CactusFeature()
        {
            Random r = new Random();
            Type = FeatureType.CACTUS;
            X = r.NextDouble();
            Y = 0;
        }
    }

    public class RockFeature : Feature
    {
        public Biome biome;

        // Parameterless constructor required for deserialization.
        public RockFeature()
        {
        }

        public RockFeature(Biome b)
        {
            Random r = new Random();
            Type = FeatureType.ROCK;
            X = r.NextDouble();
            Y = 0;
            biome = b;
        }
    }
}