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
        public static string toggleCache = "CommandToggleDraftDesc".Translate().ToString();

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            var pawn = __instance;
            bool isDraftableAnimal = pawn.IsDraftableControllableAnimal();
            bool alreadyHasVanillaDraftButton = false;
            foreach (var g in __result)
            {
                if (g is Command_Toggle command && command.defaultDesc == toggleCache)
                {
                    alreadyHasVanillaDraftButton = true;
                }
                yield return g;
            }

            if (pawn.abilities != null && !isDraftableAnimal && pawn.IsAbilityUserAnimal())
            {
                if (!DebugSettings.godMode || (DebugSettings.godMode && !DebugSettings.ShowDevGizmos))
                {
                    foreach (Ability a in pawn.abilities.AllAbilitiesForReading)
                    {

                        bool visibleSecondary = (pawn.Drafted || a.def.displayGizmoWhileUndrafted) && a.GizmosVisible();

                        foreach (Command gizmo in a.GetGizmos())
                        {
                            Command_Ability command_Ability;
                            if ((command_Ability = (gizmo as Command_Ability)) != null)
                            {
                                command_Ability.devGizmo = (!visibleSecondary && DebugSettings.ShowDevGizmos);
                            }
                            yield return gizmo;
                        }


                        foreach (Gizmo item in a.GetGizmosExtra())
                        {
                            yield return item;
                        }
                    }
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

           
            if (!alreadyHasVanillaDraftButton && isDraftableAnimal && pawn.drafter != null)
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
        }
    }
}
