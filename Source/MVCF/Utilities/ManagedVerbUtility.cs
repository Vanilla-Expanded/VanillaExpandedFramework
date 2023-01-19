using System;
using System.Runtime.CompilerServices;
using MVCF.Comps;
using Verse;

namespace MVCF.Utilities;

public static class ManagedVerbUtility
{
    private static readonly ConditionalWeakTable<Verb, ManagedVerb> managedVerbForVerbs = new();

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
        if (managedVerbForVerbs.TryGetValue(verb, out var mv)) managedVerbForVerbs.Remove(verb);
        else mv = null;
        Scribe_Deep.Look(ref mv, "MVCF_ManagedVerb");
        managedVerbForVerbs.Add(verb, mv);
    }

    public static void Register(this ManagedVerb mv)
    {
        if (managedVerbForVerbs.TryGetValue(mv.Verb, out var man))
        {
            if (man == mv) return;
            managedVerbForVerbs.Remove(mv.Verb);
        }

        managedVerbForVerbs.Add(mv.Verb, mv);
    }

    public static ManagedVerb Managed(this Verb verb, bool warnOnFailed = true)
    {
        if (verb == null) return null;
        if (managedVerbForVerbs.TryGetValue(verb, out var mv))
            return mv;

        if (warnOnFailed)
            Log.ErrorOnce("[MVCF] Attempted to get ManagedVerb for verb " + verb.Label() +
                          " which does not have one. This may cause issues.", verb.GetHashCode());

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
