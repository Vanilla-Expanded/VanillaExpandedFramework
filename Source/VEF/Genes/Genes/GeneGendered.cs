using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VEF.Genes
{
    public class GeneGendered : Gene
    {
        private GeneExtension _extension;
        public GeneExtension Extension => _extension ??= def.GetModExtension<GeneExtension>();
        public override bool Active
        {
            get
            {
                var extension = Extension;
                if (pawn != null && extension != null && pawn.gender != Extension.forGenderOnly)
                {
                    return false;
                }
                return base.Active;
            }
        }
    }
}