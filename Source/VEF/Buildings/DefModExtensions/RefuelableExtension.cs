using Verse;

namespace VEF.Buildings;

public class RefuelableExtension : DefModExtension
{
    // Normally, if using refuelable with custom fuelMultiplier or factorByDifficulty set to true,
    // the amount of fuel you get back when ejecting fuel or deconstructing the refuelable completely
    // ignores the multiplier/difficulty factor, thus ejecting too much/too little fuel.
    // Setting this value to true will cause this refuelable to consider those multiplier while ejecting.
    public bool ejectingFuelRespectsFuelMultiplier = false;
	// Data for custom fuel gauge drawing.
	// Could be done as a subtype of CompRefuelable, but using a patch means that it'll
	// work in any type of refuelable as long as it doesn't replace PostDraw method
    // Remember to disable vanilla fuel gauge drawing, as this one doesn't check for it!
    public CustomFillableBarGaugeData customFuelGauge = null;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (ejectingFuelRespectsFuelMultiplier)
            VanillaExpandedFramework_CompRefuelable_EjectFuelPatches.patchActive = true;
        if (customFuelGauge != null)
        {
            VanillaExpandedFramework_CompRefuelable_PostDraw_Patch.patchActive = true;
            customFuelGauge.ResolveReferences();
        }
    }
}