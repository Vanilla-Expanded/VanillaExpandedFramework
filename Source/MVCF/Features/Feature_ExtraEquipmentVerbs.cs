using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;
using MVCF.Comps;
using MVCF.ModCompat;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Features;

public class Feature_ExtraEquipmentVerbs : Feature_Humanoid
{
    private static readonly Func<VerbTracker, Thing, Verb, Command_VerbTarget> createVerbTargetCommand =
        AccessTools.Method(typeof(VerbTracker), "CreateVerbTargetCommand").CreateDelegate<Func<VerbTracker, Thing, Verb, Command_VerbTarget>>();

    public override string Name => "ExtraEquipmentVerbs";

    public override IEnumerable<Patch> GetPatches()
    {
        foreach (var patch in base.GetPatches()) yield return patch;

        yield return Patch.Postfix(AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
            AccessTools.Method(typeof(Feature_RangedAnimals), nameof(Feature_RangedAnimals.Pawn_GetGizmos_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(typeof(CompEquippable), nameof(CompEquippable.GetVerbsCommands)),
            AccessTools.Method(GetType(), nameof(GetVerbsCommands_Prefix)));
        yield return Patch.Prefix(AccessTools.PropertyGetter(typeof(ThingDef), nameof(ThingDef.IsRangedWeapon)),
            AccessTools.Method(GetType(), nameof(Prefix_IsRangedWeapon)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(FloatMenuMakerMap), "AddDraftedOrders"),
            AccessTools.Method(GetType(), nameof(CheckForMelee)));
    }

    public static bool GetVerbsCommands_Prefix(ref IEnumerable<Command> __result, CompEquippable __instance)
    {
        var rangedVerbs = __instance.AllVerbs.Where(v => !v.IsMeleeAttack).ToList();
        var melee = VerbManager.PreferMelee(__instance.parent);
        if (rangedVerbs.Count <= 1 && !melee && !MVCF.GetFeature<Feature_VerbComps>().Enabled) return true;
        if (DualWieldCompat.Active && __instance.parent.ParentHolder is Pawn_EquipmentTracker { pawn: { } pawn } tracker && tracker.PrimaryEq == __instance &&
            pawn.HasOffHand()) return true;
        __result = rangedVerbs
            .SelectMany(v => v.GetGizmosForVerb(v.Managed()))
            .OfType<Command>();
        if (__instance.parent.def.IsMeleeWeapon)
            __result = __result.Prepend(createVerbTargetCommand(__instance.verbTracker, __instance.parent,
                __instance.AllVerbs.First(v => v.verbProps.IsMeleeAttack)));
        return false;
    }

    public static IEnumerable<CodeInstruction> CheckForMelee(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();
        var idx = list.FindIndex(ins => ins.opcode == OpCodes.Brtrue);
        var label = list[idx].operand;
        list.InsertRange(idx + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), "equipment")),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_EquipmentTracker), "get_Primary")),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VerbManager), "PreferMelee")),
            new CodeInstruction(OpCodes.Brtrue, label)
        });
        return list;
    }

    public static bool Prefix_IsRangedWeapon(ref bool __result, ThingDef __instance)
    {
        if (__instance.IsWeapon &&
            __instance.GetCompProperties<CompProperties_VerbProps>() is CompProperties_VerbProps props &&
            props.ConsiderMelee)
        {
            __result = false;
            return false;
        }

        return true;
    }
}