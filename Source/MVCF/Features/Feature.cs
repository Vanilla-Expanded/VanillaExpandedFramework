using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.ModCompat;
using MVCF.PatchSets;
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
    }

    public virtual IEnumerable<PatchSet> GetPatchSets()
    {
        yield return new PatchSet_Base();
        yield return new PatchSet_BatteLog();
        yield return new PatchSet_TargetFinder();
        yield return new PatchSet_Equipment();
        // yield return new PatchSet_Debug();
        if (ModLister.HasActiveModWithName("RunAndGun")) yield return new PatchSet_RunAndGun();
        if (DualWieldCompat.DoNullCheck) yield return new PatchSet_DualWield();
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
}
