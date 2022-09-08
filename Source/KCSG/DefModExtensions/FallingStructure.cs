using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KCSG
{
    public struct WeightedStruct
    {
        public StructureLayoutDef structureLayoutDef;
        public float weight;

        public WeightedStruct(StructureLayoutDef structureLayoutDef, float weight)
        {
            this.structureLayoutDef = structureLayoutDef;
            this.weight = weight;
        }

        public static WeightedStruct FromString(string str)
        {
            string[] array = str.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new WeightedStruct(DefDatabase<StructureLayoutDef>.GetNamed(array[0]), array.Length == 2 ? float.Parse(array[1]) : 1f);
        }
    }

    public class FallingStructure : DefModExtension
    {
        public List<string> structures = new List<string>();
        public List<ThingDef> thingsToSpawnInDropPod = new List<ThingDef>();
        public List<FactionDef> canBeUsedBy = new List<FactionDef>();
        public bool spawnDormantWhenPossible = true;
        public bool needToHaveSettlements = true;
        public Type skyfaller = typeof(KCSG_Skyfaller);

        internal List<WeightedStruct> weightedStructs = new List<WeightedStruct>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in structures)
            {
                weightedStructs.Add(WeightedStruct.FromString(str));
            }

            foreach (var err in base.ConfigErrors())
                yield return err;

            if (structures.NullOrEmpty())
                yield return $"FallingStructure defModExtension can't have an empty structures list";

            if (canBeUsedBy.NullOrEmpty())
                yield return $"FallingStructure defModExtension can't have an empty canBeUsedBy list";

        }
    }
}