using Verse;

namespace VEF.Buildings;

public class RefuelableExtension : DefModExtension
{
    public bool ejectingFuelRespectsFuelMultiplier = false;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (ejectingFuelRespectsFuelMultiplier)
            VanillaExpandedFramework_CompRefuelable_EjectFuelPatches.patchActive = true;
    }
}