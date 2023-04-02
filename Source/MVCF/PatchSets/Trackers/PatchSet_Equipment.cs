using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Features;
using MVCF.ModCompat;
using MVCF.Utilities;
using Verse;

namespace MVCF.PatchSets.Trackers;

public class PatchSet_Equipment : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        // Track pawn equipment changes if any feature is enabled to ensure that verbs selected by OrderForceTarget()
        // correctly reflect changes to equipment.
        var equipmentTrackerType = typeof(Pawn_EquipmentTracker);
        yield return Patch.Postfix(AccessTools.Method(equipmentTrackerType, "Notify_EquipmentAdded"),
            AccessTools.Method(GetType(), nameof(EquipmentAdded_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(equipmentTrackerType, "Notify_EquipmentRemoved"),
            AccessTools.Method(GetType(), nameof(EquipmentRemoved_Prefix)));
        yield return Patch.Postfix(AccessTools.Method(equipmentTrackerType, "TryTransferEquipmentToContainer"),
            AccessTools.Method(GetType(), nameof(TryTransferEquipmentToContainer_Postfix)));
    }

    public static void EquipmentAdded_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
    {
        __instance.pawn.Manager(false)?.AddVerbs(eq);
    }

    public static void EquipmentRemoved_Prefix(ThingWithComps eq, Pawn_EquipmentTracker __instance) => MaybeRemoveEquipmentVerb(eq, __instance);

    /// <summary>
    ///     Called when a piece of equipment is transferred from a pawn's
    ///     <see cref="Pawn_EquipmentTracker">EquipmentTracker</see> to another container.
    /// </summary>
    /// <remarks>
    ///     This can occur in some edge cases in vanilla (e.g. a pawn dying or getting downed inside a shuttle / transport
    ///     pod), but may also be used by mods.
    /// </remarks>
    public static void TryTransferEquipmentToContainer_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance) => MaybeRemoveEquipmentVerb(eq, __instance);

    /// <summary>
    ///     Attempt to remove the verb(s) of a piece of equipment from the <see cref="VerbManager" /> of the owning pawn.
    /// </summary>
    private static void MaybeRemoveEquipmentVerb(ThingWithComps eq, Pawn_EquipmentTracker __instance)
    {
        if (MVCF.IsIgnoredMod(eq?.def?.modContentPack?.Name)) return;
        if (MVCF.ShouldIgnore(eq)) return;
        if (DualWieldCompat.Active && eq.IsOffHand()) return;
        var comp = eq.TryGetComp<CompEquippable>();
        if (comp?.VerbTracker?.AllVerbs == null) return;
        var manager = __instance?.pawn?.Manager(false);
        if (manager == null) return;
        if (MVCF.GetFeature<Feature_ExtraEquipmentVerbs>().Enabled)
            foreach (var verb in comp.VerbTracker.AllVerbs.Concat(manager.ExtraVerbsFor(eq)))
                manager.RemoveVerb(verb);
        else manager.RemoveVerb(comp.PrimaryVerb);
    }
}
