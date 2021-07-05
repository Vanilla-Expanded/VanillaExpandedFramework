using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct WeightedStruct
    {
        public float weight;
        public StructureLayoutDef structureLayoutDef;

        public WeightedStruct(StructureLayoutDef structureLayoutDef, float weight)
        {
            this.structureLayoutDef = structureLayoutDef;
            this.weight = weight;
        }

        public static WeightedStruct FromString(string str)
        {
            str = str.TrimStart(new char[]
            {
                '('
            });
            str = str.TrimEnd(new char[]
            {
                ')'
            });
            string[] array = str.Split(new char[]
            {
                ','
            });

            if (array.Length == 2)
            {
                return new WeightedStruct(DefDatabase<StructureLayoutDef>.GetNamedSilentFail(array[0]), float.Parse(array[1].TrimStart(new char[] { ' ' })));
            }
            else return new WeightedStruct(DefDatabase<StructureLayoutDef>.GetNamedSilentFail(array[0]), 1f);
        }
    }

    public class FallingStructure : DefModExtension
    {
        public List<string> weightedStruct = new List<string>();
        public bool spawnDormantWhenPossible = true;

        public List<FactionDef> canBeUsedBy = new List<FactionDef>();

        public List<WeightedStruct> WeightedStructs = new List<WeightedStruct>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in weightedStruct)
            {
                WeightedStructs.Add(WeightedStruct.FromString(str));
            }

            if (WeightedStructs.Count == 0)
            {
                Log.Error($"FallingStructure defModExtension can't have an empty or null WeightedStructs");
            }
            if (canBeUsedBy.Count == 0)
            {
                Log.Error($"FallingStructure defModExtension can't have an empty or null canBeUsedBy");
            }
            return base.ConfigErrors();
        }
    }
}