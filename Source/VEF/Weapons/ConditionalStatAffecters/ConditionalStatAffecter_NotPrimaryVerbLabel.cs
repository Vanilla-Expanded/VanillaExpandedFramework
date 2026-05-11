using Verse;

namespace VEF.Weapons;

public class ConditionalStatAffecter_NotPrimaryVerbLabel : ConditionalStatAffecter_PrimaryVerbLabel
{
    protected override bool IsApplicableVerbs(Verb verb)
    {
        // Invert the result. Only applies if the selected verb is not present.
        return !base.IsApplicableVerbs(verb);
    }
}