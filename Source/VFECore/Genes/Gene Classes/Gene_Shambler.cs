using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{
    public class Gene_Shambler : Gene
    {
        public override bool Active
        {
            get
            {
                if (!pawn.IsShambler)
                {
                    return false;
                }
                return base.Active;
            }
        }
    }
}