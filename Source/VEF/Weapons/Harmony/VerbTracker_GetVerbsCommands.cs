using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VEF.Weapons;

[HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.GetVerbsCommands))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_VerbTracker_GetVerbsCommands_Patch
{
    private static bool Prepare() => VanillaExpandedFramework_CompEquippable_PrimaryVerb_Patch.IsActive;

    private static IEnumerable<Command> Postfix(IEnumerable<Command> commands, VerbTracker __instance)
    {
        if (__instance.directOwner is not CompEquippable equippable || equippable.parent.GetComp<CompMultiVerbWeapon>() is not {} comp)
        {
            foreach (var command in commands)
                yield return command;
            yield break;
        }

        foreach (var command in commands)
        {
            // Only return the verbs if: the verb is not Command_VerbTarget (patched command),
            // it is the active verb, the verb is not contained in our supported verb list.
            if (command is not Command_VerbTarget target || target.verb.verbProps.untranslatedLabel == comp.ActiveVerbData.verbLabel || !comp.Props.verbs.Any(d => d.verbLabel == target.verb.verbProps.untranslatedLabel))
                yield return command;
        }

        foreach (var gizmo in comp.CompGetSwitchModeGizmo())
            yield return gizmo;
    }
}