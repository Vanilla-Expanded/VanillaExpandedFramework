using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Weapons;

public class ConditionalStatAffecter_PrimaryVerbLabel : ConditionalStatAffecter
{
    public string explanationLabel;
    public List<string> applicableVerbLabels;

    public override string Label => explanationLabel;

    public override bool Applies(StatRequest req)
    {
        // Doesn't apply if null or has no comps
        if (req.Thing is not ThingWithComps thing)
            return false;

        // We only support CompEquippable
        if (thing.GetComp<CompEquippable>() is not {} comp)
            return false;

        // If no primary verb, do nothing
        var verb = comp.PrimaryVerb;
        if (verb == null)
            return false;

        // Check if the verb is applicable here
        return IsApplicableVerbs(verb);
    }

    protected virtual bool IsApplicableVerbs(Verb verb)
    {
        // Make sure that the list isn't null
        if (applicableVerbLabels != null)
        {
            // Check if the (untranslated) verb label matches any of the ones we're supporting
            for (var i = 0; i < applicableVerbLabels.Count; i++)
            {
                if (applicableVerbLabels[i] == verb.verbProps.untranslatedLabel)
                    return true;
            }
        }

        return false;
    }
}