using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static readonly Texture2D CreateTex = ContentFinder<Texture2D>.Get("UI/Gizmo/SetUpOutpost");

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
    }
}