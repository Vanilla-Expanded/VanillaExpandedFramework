using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct StructOption
    {
        public int maxCount;
        public int minCount;
        public string structureLayoutTag;
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
        public bool addLandingPad = false;
        public List<string> allowedStructures = new List<string>();
        public List<StructOption> allowedStructuresConverted = new List<StructOption>();
        public List<string> atLeastOneOfTags = new List<string>();
        public PawnGroupKindDef groupKindDef = null;
        public TerrainDef mainRoadDef = null;
        public float pawnGroupMultiplier = 1f;
        public TerrainDef roadDef = null;
        public IntVec2 settlementSize = new IntVec2(42, 42);
        public float stockpileValueMultiplier = 1f;
        public bool vanillaLikeDefense = false;
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in allowedStructures)
            {
                allowedStructuresConverted.Add(StructOption.FromString(str));
            }

            return base.ConfigErrors();
        }
    }
}