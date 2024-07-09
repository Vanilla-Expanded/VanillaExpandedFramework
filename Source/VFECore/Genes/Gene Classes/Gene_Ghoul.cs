using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{
    public class Gene_Ghoul : Gene
    {
       
        public override bool Active
        {
            get
            {
                if (!pawn.IsGhoul)
                {
                    return false;
                }
                return base.Active;
            }
        }
    }
}