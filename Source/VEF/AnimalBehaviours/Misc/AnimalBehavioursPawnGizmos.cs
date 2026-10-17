using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.AnimalBehaviours;

public interface PawnGizmoProvider
{
    IEnumerable<Gizmo> GetGizmos();
}

public static class AnimalBehavioursPawnGizmos
{
    public static string toggleCache = "CommandToggleDraftDesc".Translate().ToString();

    public static IEnumerable<Gizmo> GetGizmos(Pawn pawn, IEnumerable<Gizmo> gizmos)
    {
        bool isDraftableAnimal = pawn.IsDraftableControllableAnimal();
        bool alreadyHasVanillaDraftButton = false;
        foreach (var g in gizmos)
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

        if (!alreadyHasVanillaDraftButton && isDraftableAnimal && pawn.drafter != null)
        {
            Command_Toggle drafting_command = new Command_Toggle();
            drafting_command.toggleAction = delegate { pawn.drafter.Drafted = !pawn.drafter.Drafted; };
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

        foreach (var comp in pawn.AllComps)
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