using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Features.PatchSets;
using MVCF.ModCompat;
using MVCF.ModCompat.PatchSets;
using MVCF.Utilities;
using MVCF.VerbComps;
using Verse;

namespace MVCF.Features;

public abstract class Feature
{
    public bool Enabled;
    public abstract string Name { get; }
    public IEnumerable<Patch> Patches => GetPatches().Concat(GetPatchSets().SelectMany(set => set.GetPatches()));

    public virtual IEnumerable<Patch> GetPatches()
    {
        yield return Patch_Pawn_TryGetAttackVerb.GetPatch();
        yield return Patch.Postfix(AccessTools.Method(typeof(Pawn), nameof(Pawn.ExposeData)), AccessTools.Method(GetType(), nameof(PostExposeDataPawn)));
        yield return Patch.Postfix(AccessTools.Method(typeof(Verb), nameof(Verb.ExposeData)), AccessTools.Method(GetType(), nameof(PostExposeDataVerb)));
        yield return Patch.Postfix(AccessTools.Method(typeof(VerbTracker), "InitVerb"), AccessTools.Method(GetType(), nameof(PostInitVerb)));
        yield return Patch.Postfix(AccessTools.Method(typeof(VerbTracker), "InitVerb"), AccessTools.Method(GetType(), nameof(PostInitVerb)));
        foreach (var patch in TargetFinder.GetPatches()) yield return patch;

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

    public virtual IEnumerable<PatchSet> GetPatchSets()
    {
        yield return new PatchSet_BatteLog();
        yield return new PatchSet_Base();
        // yield return new PatchSet_Debug();
        if (ModLister.HasActiveModWithName("RunAndGun")) yield return new PatchSet_RunAndGun();
    }

    public static void PostExposeDataPawn(Pawn __instance)
    {
        __instance.SaveManager();
    }

    public static void PostExposeDataVerb(Verb __instance)
    {
        __instance.SaveManaged();
    }

    public static void PostInitVerb(VerbTracker __instance, Verb verb)
    {
        AdditionalVerbProps props;
        IEnumerable<VerbCompProperties> additionalComps;
        switch (__instance.directOwner)
        {
            case CompEquippable comp:
                props = comp.props is CompProperties_VerbProps compProps
                    ? compProps.PropsFor(verb)
                    : comp.parent.TryGetComp<Comp_VerbProps>()?.Props?.PropsFor(verb);
                additionalComps = comp.parent.AllComps.OfType<VerbComp.IVerbCompProvider>().SelectMany(p => p.GetCompsFor(verb.verbProps));
                break;
            case HediffComp_VerbGiver comp:
                props = (comp as HediffComp_ExtendedVerbGiver)?.PropsFor(verb);
                additionalComps = comp.parent.comps.OfType<VerbComp.IVerbCompProvider>().SelectMany(p => p.GetCompsFor(verb.verbProps));
                break;
            case Comp_VerbGiver comp:
                props = comp.PropsFor(verb);
                additionalComps = comp.parent.AllComps.OfType<VerbComp.IVerbCompProvider>().SelectMany(p => p.GetCompsFor(verb.verbProps));
                break;
            case Pawn pawn:
                props = pawn.TryGetComp<Comp_VerbProps>()?.PropsFor(verb);
                additionalComps = pawn.AllComps.OfType<VerbComp.IVerbCompProvider>().SelectMany(p => p.GetCompsFor(verb.verbProps));
                break;
            case CompVerbsFromInventory comp:
                props = comp.PropsFor(verb);
                additionalComps = comp.parent.AllComps.OfType<VerbComp.IVerbCompProvider>().SelectMany(p => p.GetCompsFor(verb.verbProps));
                break;
            default: return;
        }

        var comps = additionalComps.ToList();
        var mv = verb.Managed(false) ?? props.CreateManaged(!((props is null || props.comps.NullOrEmpty()) && comps.NullOrEmpty()));
        mv.Initialize(verb, props, comps);
    }

    public static void EquipmentAdded_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
    {
        __instance.pawn.Manager(false)?.AddVerbs(eq);
    }

    public static void EquipmentRemoved_Prefix(ThingWithComps eq, Pawn_EquipmentTracker __instance) => MaybeRemoveEquipmentVerb(eq, __instance);

    /// <summary>
    /// Called when a piece of equipment is transferred from a pawn's <see cref="Pawn_EquipmentTracker">EquipmentTracker</see> to another container.
    /// </summary>
    /// <remarks>
    /// This can occur in some edge cases in vanilla (e.g. a pawn dying or getting downed inside a shuttle / transport pod), but may also be used by mods.
    /// </remarks>
    public static void TryTransferEquipmentToContainer_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance) => MaybeRemoveEquipmentVerb(eq, __instance);

    /// <summary>
    /// Attempt to remove the verb(s) of a piece of equipment from the <see cref="VerbManager"/> of the owning pawn.
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
