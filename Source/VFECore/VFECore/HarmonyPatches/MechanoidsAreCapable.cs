using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(JobGiver_Work), "PawnCanUseWorkGiver")]
    public static class MechanoidsAreCapable
    {
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer)
            {
                __result = true;
            }
        }
    }
}
