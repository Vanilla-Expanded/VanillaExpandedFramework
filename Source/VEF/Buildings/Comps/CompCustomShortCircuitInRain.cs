using Verse;

namespace VEF.Buildings;

public class CompProperties_CustomShortCircuitInRain : CompProperties
{
    // True will always trigger the vanilla short circuit when this is picked for short circuit from rain,
    // even if this building normally couldn't short circuit in rain. False will cause the short circuit
    // to never trigger, even if the building would normally support it (like a battery).
    // For more fine-tuning and/or custom behavior, you'll need to add some custom code in a subclass.
    public bool canVanillaShortCircuit = true;

    public CompProperties_CustomShortCircuitInRain() => compClass = typeof(CompCustomShortCircuitInRain);

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);

        VanillaExpandedFramework_ShortCircuitUtility_TryShortCircuitInRain_Patch.patchActive = true;
    }
}

public class CompCustomShortCircuitInRain : ThingComp
{
    public CompProperties_CustomShortCircuitInRain Props => (CompProperties_CustomShortCircuitInRain)props;

    public virtual bool VanillaShortCircuit => true;

    public virtual bool CanVanillaShortCircuitNow => Props.canVanillaShortCircuit;

    public virtual bool TriggerCustomShortCircuit() => false;
}