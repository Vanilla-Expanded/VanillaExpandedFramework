using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MonoMod.Utils;
using MVCF.Features;
using MVCF.ModCompat;
using MVCF.Utilities;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_ExtraEquipment : PatchSet
{
    private static readonly Func<VerbTracker, Thing, Verb, Command_VerbTarget> createVerbTargetCommand =
        AccessTools.Method(typeof(VerbTracker), "CreateVerbTargetCommand").CreateDelegate<Func<VerbTracker, Thing, Verb, Command_VerbTarget>>();

    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(CompEquippable), nameof(CompEquippable.GetVerbsCommands)),
            AccessTools.Method(GetType(), nameof(GetVerbsCommands_Prefix)));
    }

    public static bool GetVerbsCommands_Prefix(ref IEnumerable<Command> __result, CompEquippable __instance)
    {
        var rangedVerbs = __instance.AllVerbs.Where(v => !v.IsMeleeAttack).ToList();
        var melee = __instance.parent.PrefersMelee();
        if (rangedVerbs.Count <= 1 && !melee && !MVCF.GetFeature<Feature_VerbComps>().Enabled) return true;
        if (DualWieldCompat.Active && __instance.parent.ParentHolder is Pawn_EquipmentTracker { pawn: { } pawn } tracker && tracker.PrimaryEq == __instance
          &&
            pawn.HasOffHand()) return true;
        __result = rangedVerbs
           .Select(v => v.GetGizmosForVerb(v.Managed()))
           .OrderByDescending(cs => cs.Any(c => c is Command_VerbTarget))
           .SelectMany(cs => cs)
           .OfType<Command>();
        if (__instance.parent.def.IsMeleeWeapon)
            __result = __result.Prepend(createVerbTargetCommand(__instance.verbTracker, __instance.parent,
                __instance.AllVerbs.First(v => v.verbProps.IsMeleeAttack)));
        return false;
    }
}
