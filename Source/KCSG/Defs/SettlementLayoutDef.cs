using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct StructOption
    {
        public string structureLayoutTag;

        public int minCount;

        public int maxCount;

        public StructOption(string structureLayoutTag, int minCount, int maxCount = -1)
        {
            this.structureLayoutTag = structureLayoutTag;
            this.minCount = minCount;
            this.maxCount = maxCount;
        }

        public static StructOption FromString(string str)
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
            int minCount = Convert.ToInt32(array[1]);
            if (array.Length == 3)
                return new StructOption(array[0], minCount, Convert.ToInt32(array[2]));
            else
                return new StructOption(array[0], minCount);
        }
    }

    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public List<string> atLeastOneOfTags = new List<string>();
        public List<string> allowedStructures = new List<string>();
        public List<StructOption> allowedStructuresConverted = new List<StructOption>();

        public TerrainDef roadDef = null;
        public TerrainDef mainRoadDef = null;

        public bool vanillaLikeDefense = false;
        public bool addLandingPad = false;

        public override IEnumerable<string> ConfigErrors()
        {
            if (settlementSize.x > 200)
            {
                settlementSize.x = 200;
                Log.Warning($"SettlementLayoutDef {defName} has inccorect x size of {settlementSize}, max is 200. Changing to 200...");
            }
            if (settlementSize.z > 200)
            {
                settlementSize.z = 200;
                Log.Warning($"SettlementLayoutDef {defName} has inccorect y size of {settlementSize}, max is 200. Changing to 200...");
            }

            foreach (string str in allowedStructures)
            {
                allowedStructuresConverted.Add(StructOption.FromString(str));
            }

            return base.ConfigErrors();
        }
    }
}