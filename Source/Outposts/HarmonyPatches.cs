using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static readonly Texture2D CreateTex = ContentFinder<Texture2D>.Get("UI/Gizmo/SetUpOutpost");

        public static void DoPatches()
        {
            OutpostsMod.Harm.Patch(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetGizmos)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AddCaravanGizmos)));
            OutpostsMod.Harm.Patch(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetInspectString)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AddRestingAtOutpost)));
            OutpostsMod.Harm.Patch(AccessTools.Method(typeof(Translator), nameof(Translator.Translate), new[] {typeof(string)}),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Debug)));
        }

        public static void Debug(string key)
        {
            if (key == "CaravanDetectedRaidCountdownTip") Log.Message($"Found key at {new StackTrace()}");
        }

        public static IEnumerable<Gizmo> AddCaravanGizmos(IEnumerable<Gizmo> gizmos, Caravan __instance)
        {
            foreach (var gizmo in gizmos) yield return gizmo;

            yield return new Command_Action
            {
                action = () => Find.WindowStack.Add(new Dialog_CreateCamp(__instance)),
                defaultLabel = "Outposts.Commands.Create.Label".Translate(),
                defaultDesc = "Outposts.Commands.Create.Desc".Translate(),
                icon = CreateTex
            };
        }

        public static void AddRestingAtOutpost(Caravan __instance, ref string __result)
        {
            if (!__instance.pather.MovingNow && Find.WorldObjects.WorldObjectAt<Outpost>(__instance.Tile) is Outpost outpost)
                __result += "\n" + "Outposts.RestingAt".Translate(outpost.Name);
        }
    }
}