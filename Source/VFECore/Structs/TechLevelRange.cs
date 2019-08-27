using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public struct TechLevelRange : IEquatable<TechLevelRange>
    {

        public TechLevelRange(TechLevel min, TechLevel max)
        {
            this.min = min;
            this.max = max;
        }

        public static TechLevelRange All
        {
            get
            {
                return new TechLevelRange(TechLevel.Animal, TechLevel.Archotech);
            }
        }

        public bool Includes(TechLevel level)
        {
            return level >= min && level <= max;
        }

        public static bool operator ==(TechLevelRange a, TechLevelRange b)
        {
            return a.min == b.min && a.max == b.max;
        }

        public static bool operator !=(TechLevelRange a, TechLevelRange b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"{min}~{max}";
        }

        public static TechLevelRange FromString(string s)
        {
            var array = s.Split('~');
            return new TechLevelRange(ParseHelper.FromString<TechLevel>(array[0]), ParseHelper.FromString<TechLevel>(array[1]));
        }

        public override bool Equals(object obj)
        {
            return obj is TechLevelRange tlr && this == tlr;
        }

        public bool Equals(TechLevelRange other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return Gen.HashCombineStruct(min.GetHashCode(), max);
        }

        public TechLevel min;
        public TechLevel max;

    }

}
