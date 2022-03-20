using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI.Group;
using System.Reflection;
using VFE.Mechanoids;
using VFEMech;
using VFE.Mechanoids.HarmonyPatches;

namespace AnimalBehaviours
{
    public interface PawnGizmoProvider
    {
        IEnumerable<Gizmo> GetGizmos();
    }
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Patch
    {
        private static MethodInfo getGizmosDraftController = typeof(Pawn_DraftController).GetMethod("GetGizmos", BindingFlags.Instance | BindingFlags.NonPublic);
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            var pawn = __instance;
            bool shouldbeDraftable = pawn.IsDraftableControllableAnimal();
            bool abilityUser = pawn.IsAbilityUserAnimal();
            foreach (var g in __result)
            {
                if (shouldbeDraftable && g is Command_Toggle command && command.defaultDesc == "CommandToggleDraftDesc".Translate())
                {
                    continue;
                }
                yield return g;
            }

            if (abilityUser && pawn.abilities != null)
            {
                foreach (Gizmo gizmoAbility in pawn.abilities.GetGizmos())
                {
                    yield return gizmoAbility;
                }
            }

            if (SimpleSidearmsPatch.SimpleSidearmsActive && __instance is Machine)
            {
                var compMachine = pawn.GetComp<CompMachine>();
                if (compMachine != null && compMachine.Props.canPickupWeapons)
                {
                    foreach (var g in SimpleSidearmsPatch.SimpleSidearmsGizmos(__instance))
                    {
                        yield return g;
                    }
                }
            }

            if (shouldbeDraftable && pawn.drafter != null)
            {
                Command_Toggle drafting_command = new Command_Toggle();
                drafting_command.toggleAction = delegate
                {
                    pawn.drafter.Drafted = !pawn.drafter.Drafted;
                };
                drafting_command.isActive = (() => pawn.drafter.Drafted);
                drafting_command.defaultLabel = (pawn.drafter.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate();
                drafting_command.hotKey = KeyBindingDefOf.Command_ColonistDraft;
                drafting_command.defaultDesc = "CommandToggleDraftDesc".Translate();
                drafting_command.icon = ContentFinder<Texture2D>.Get("ui/commands/Draft", true);
                drafting_command.turnOnSound = SoundDefOf.DraftOn;
                drafting_command.groupKey = 81729172;
                drafting_command.turnOffSound = SoundDefOf.DraftOff;
                yield return drafting_command;
            }

            foreach (var comp in __instance.AllComps)
            {
                if (comp is PawnGizmoProvider gizmoProvider)
                {
                    foreach (var gizmo in gizmoProvider.GetGizmos())
                    {
                        yield return gizmo;
                    }
                }
            }

            if (__instance.RaceProps.IsMechanoid && __instance.Faction == Faction.OfPlayer && __instance.drafter != null
                && CompMachine.cachedMachines.ContainsKey(__instance.Drawer.renderer))
            {
                IEnumerable<Gizmo> collection = (IEnumerable<Gizmo>)getGizmosDraftController.Invoke(__instance.drafter, new object[0]);
                foreach (var gizmo in collection)
                {
                    yield return gizmo;
                }
            }
        }
    }
}
