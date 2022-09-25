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
        public static void Postfix(ref bool __result, Pawn pawn, WorkGiver giver)
        {
            if (pawn is Machine && CompMachine.cachedMachinesPawns.TryGetValue(pawn, out CompMachine comp))
            {
                var chargingComp = comp?.myBuilding?.TryGetComp<CompMachineChargingStation>();
                if (chargingComp != null)
                {
                    if (__result && chargingComp.Props.disallowedWorkGivers != null && chargingComp.Props.disallowedWorkGivers.Contains(giver.def))
                    {
                        __result = false;
                    }
                    else if (__result is false && pawn.def.race.mechEnabledWorkTypes != null 
                        && pawn.def.race.mechEnabledWorkTypes.Contains(giver.def.workType))
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}
