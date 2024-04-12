using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Outposts
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static void DoPatches()
        {
            OutpostsMod.Harm.Patch(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetGizmos)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AddCaravanGizmos)));
            OutpostsMod.Harm.Patch(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetInspectString)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AddRestingAtOutpost)));
        }

        public static IEnumerable<Gizmo> AddCaravanGizmos(IEnumerable<Gizmo> gizmos, Caravan __instance)
        {
            foreach (var gizmo in gizmos) yield return gizmo;

            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(__instance.Tile) ||
                Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Any(outpost => Find.WorldGrid.IsNeighborOrSame(__instance.Tile, outpost.Tile)))
                yield return new Command_Action
                {
                    action = () => { },
                    defaultLabel = "Outposts.Commands.Create.Label".Translate(),
                    defaultDesc = "Outposts.Commands.Create.Desc".Translate(),
                    icon = TexOutposts.CreateTex,
                    Disabled = true,
                    disabledReason = "Outposts.TooClose".Translate()
                };
            else
                yield return new Command_Action
                {
                    action = () => Find.WindowStack.Add(new Dialog_CreateCamp(__instance)),
                    defaultLabel = "Outposts.Commands.Create.Label".Translate(),
                    defaultDesc = "Outposts.Commands.Create.Desc".Translate(),
                    icon = TexOutposts.CreateTex
                };
        }

        public static void AddRestingAtOutpost(Caravan __instance, ref string __result)
        {
            if (!__instance.pather.MovingNow && Find.WorldObjects.WorldObjectAt<Outpost>(__instance.Tile) is Outpost outpost)
                __result += "\n" + "Outposts.RestingAt".Translate(outpost.Name);
        }
    }
}