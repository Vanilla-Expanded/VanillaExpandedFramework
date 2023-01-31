using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{
    public class GeneGendered : Gene
    {
        public override bool Active
        {
            get
            {
                var extension = def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                    if (extension.forGenderOnly.HasValue && pawn.gender != extension.forGenderOnly.Value)
                    {
                        return false;
                    }
                }
                return base.Active;
            }
        }
    }
}