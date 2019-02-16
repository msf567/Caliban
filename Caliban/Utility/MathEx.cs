using System;

namespace Caliban.Core.Utility
{
    public static class MathEx
    {
        public static T Clamp<T>(this T _val, T _min, T _max) where T : IComparable<T>
        {
            if (_val.CompareTo(_min) < 0) return _min;
            else if (_val.CompareTo(_max) > 0) return _max;
            else return _val;
        }
    }
}