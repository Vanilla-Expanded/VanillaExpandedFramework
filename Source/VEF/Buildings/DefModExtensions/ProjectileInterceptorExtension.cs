using System.Collections.Generic;
using VEF.Apparels;
using Verse;

namespace VEF.Buildings;

public class ProjectileInterceptorExtension : DefModExtension
{
    // Makes the RW interceptor change color similar to our CompShieldField
    public List<HealthColorPoint> healthColorPoints;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (!healthColorPoints.NullOrEmpty())
            VanillaExpandedFramework_CompProjectileInterceptor_PostDraw_Patch.patchActive = true;
    }
}