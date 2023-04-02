using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MVCF.Comps;
using MVCF.VerbComps;
using Prepatcher;
using Verse;

namespace MVCF.Utilities;

public static class ManagedVerbUtility
{
    private static readonly ConditionalWeakTable<Verb, StrongBox<ManagedVerb>> managedVerbForVerbs = new();

    [PrepatcherField]
    private static ref ManagedVerb ManagedVerb(this Verb verb)
    {
        if (!managedVerbForVerbs.TryGetValue(verb, out var box))
        {
            box = new StrongBox<ManagedVerb>();
            managedVerbForVerbs.Add(verb, box);
        }

        return ref box.Value;
    }

    public static ManagedVerb CreateManaged(this AdditionalVerbProps props, bool hasComps)
    {
        var mv = props switch
        {
            { managedClass: { } type } => (ManagedVerb)Activator.CreateInstance(type),
            _ when hasComps => new VerbWithComps(),
            _ => new ManagedVerb()
        };

        return mv;
    }

    public static void SaveManaged(this Verb verb)
    {
        Scribe_Deep.Look(ref verb.ManagedVerb(), "MVCF_ManagedVerb");
    }

    public static void InitializeManaged(this Verb verb, VerbTracker tracker)
    {
        AdditionalVerbProps props;
        IEnumerable<VerbCompProperties> additionalComps;
        switch (tracker.directOwner)
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
        verb.ManagedVerb() = mv;
    }

    public static ManagedVerb Managed(this Verb verb, bool warnOnFailed = true)
    {
        if (verb == null) return null;
        var mv = verb.ManagedVerb();
        if (mv != null)
            return mv;

        if (warnOnFailed)
            Log.ErrorOnce($"[MVCF] Attempted to get ManagedVerb for verb {verb.Label()} which does not have one. This may cause issues.", verb.GetHashCode());

        return null;
    }

    public static Thing ParentThing(this ManagedVerb verb)
    {
        return verb.Verb.DirectOwner switch
        {
            CompEquippable comp => comp.parent,
            HediffComp_VerbGiver comp => comp.Pawn,
            Comp_VerbGiver comp => comp.parent,
            Pawn pawn => pawn,
            CompVerbsFromInventory comp => comp.parent,
            _ => null
        };
    }
}
