using System.Collections.Generic;
using System.Linq;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Comps;

public class HediffCompProperties_ExtendedVerbGiver : HediffCompProperties_VerbGiver
{
    public List<AdditionalVerbProps> verbProps;
    private HediffDef parent;

    public HediffCompProperties_ExtendedVerbGiver() => compClass = typeof(HediffComp_ExtendedVerbGiver);

    public override void PostLoad()
    {
        base.PostLoad();
        if (verbProps != null)
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                foreach (var props in verbProps)
                    props?.Initialize(parent.comps.OfType<HediffCompProperties_VerbGiver>().FirstOrDefault()?.verbs.FirstOrDefault(v => v.label == props.label),
                        parent);
            });
    }

    public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef)) yield return error;

        if (verbProps == null)
            yield return "No verbProps provided!";
        else
            foreach (var error in verbProps.SelectMany(prop => prop.ConfigErrors(parentDef)))
                yield return error;
    }

    public override void ResolveReferences(HediffDef parent)
    {
        base.ResolveReferences(parent);
        this.parent = parent;
        foreach (var verbProp in verbProps) verbProp.ResolveReferences(parent);
    }
}

public class CompProperties_VerbGiver : CompProperties_VerbProps
{
    public CompProperties_VerbGiver() => compClass = typeof(Comp_VerbGiver);
}

public class Comp_VerbProps : ThingComp
{
    public CompProperties_VerbProps Props => props as CompProperties_VerbProps;

    public AdditionalVerbProps PropsFor(Verb verb) => Props.PropsFor(verb);
}

public class CompProperties_VerbProps : CompProperties
{
    public bool ConsiderMelee;
    public List<AdditionalVerbProps> verbProps;

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);
        if (verbProps != null)
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                foreach (var props in verbProps) props?.Initialize(parent.Verbs.FirstOrDefault(v => v.label == props.label), parent);
            });
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef)) yield return error;

        if (verbProps == null)
            yield return "No verbProps provided!";
        else
            foreach (var error in verbProps.SelectMany(prop => prop.ConfigErrors(parentDef)))
                yield return error;
    }

    public AdditionalVerbProps PropsFor(Verb verb)
    {
        var label = verb.verbProps.label ?? verb.tool.label;
        return string.IsNullOrEmpty(label) ? null : verbProps?.FirstOrDefault(prop => prop.label == label);
    }

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        foreach (var verbProp in verbProps) verbProp.ResolveReferences(parentDef);
    }
}
