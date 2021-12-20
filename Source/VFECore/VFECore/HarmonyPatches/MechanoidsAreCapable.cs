using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFEMech;

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

	[HarmonyPatch(typeof(JobGiver_Work), "PawnCanUseWorkGiver")]
	public class JobGiver_Work_PawnCanUseWorkGiver_Patch
    {
		public static void Postfix(ref bool __result, Pawn pawn, WorkGiver giver)
		{
            if (pawn is Machine && CompMachine.cachedMachinesPawns.TryGetValue(pawn, out CompMachine comp))
            {
                var chargingComp = comp?.myBuilding?.TryGetComp<CompMachineChargingStation>();
                if (chargingComp != null && (chargingComp.Props.disallowedWorkGivers?.Contains(giver.def) ?? false))
                {
                    __result = false;
                }
            }
        }
	}
}
