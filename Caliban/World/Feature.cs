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
        public int Seed;
    }

    public class CactusFeature : Feature
    {
        public CactusFeature()
        {
            Random r = new Random();
            X = r.NextDouble();
            Y = 0;
            Seed = r.Next(100);
        }
    }

    public class RockFeature : Feature
    {
        public RockFeature()
        {
            Random r = new Random();
            X = r.NextDouble();
            Y = 0;
            Seed = r.Next(100);
        }
    }
}