using System;

namespace Caliban.Core.World
{
    public enum FeatureType
    {
        CACTUS,
        ROCK,
    }

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