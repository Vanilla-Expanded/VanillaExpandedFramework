using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
    [HarmonyPatch]
    public static class ResolveRaidStrategy_Patch
    {
        [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "ResolveRaidStrategy")]
        [HarmonyPostfix]
        public static void Postfix(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            Map map = (Map)parms.target;
            Faction fac = parms.faction;
            if (fac.def.GetModExtension<FactionDefExtension>() is FactionDefExtension ext && ext != null && ext.allowedStrategies?.Count > 0)
            {
                DefDatabase<RaidStrategyDef>.AllDefs.Where(d => d.Worker.CanUseWith(parms, groupKind) && ext.allowedStrategies.Contains(d) && d.arriveModes != null && d.arriveModes.Any(x => x.Worker.CanUseWith(parms)))
                    .TryRandomElementByWeight(d => d.Worker.SelectionWeight(map, parms.points), out RaidStrategyDef result);
                parms.raidStrategy = result;

                if (parms.raidStrategy != null)
                    return;
                Log.Error("No raid stategy found, defaulting to ImmediateAttack. Faction=" + parms.faction.def.defName + ", points=" + parms.points + ", groupKind=" + groupKind + ", parms=" + parms);
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            }
        }
    }
}
