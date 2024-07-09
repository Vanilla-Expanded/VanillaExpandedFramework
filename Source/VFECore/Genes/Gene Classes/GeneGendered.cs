using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{
    public class GeneGendered : Gene
    {
        private GeneExtension _extension;
        public GeneExtension Extension => _extension ??= def.GetModExtension<GeneExtension>();
        public override bool Active
        {
            get
            {
                if (pawn.gender != Extension.forGenderOnly)
                {
                    return false;
                }
                return base.Active;
            }
        }
    }
}