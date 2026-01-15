using Verse;

namespace VEF.Weapons;

public class PlaceWorker_ExpandedShowTurretRadius : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var extension = checkingDef?.GetModExtension<ExpandedShowTurretRadiusExtension>();
        var props = (checkingDef as ThingDef)?.building?.turretGunDef?.Verbs.Find(v => IsValidVerb(v, extension));

        if (props != null)
        {
            if (props.range > 0 && (extension == null || extension.drawMaxRange))
                GenDraw.DrawRadiusRing(loc, props.range);
            if (props.minRange > 0 && (extension == null || extension.drawMinRange))
                GenDraw.DrawRadiusRing(loc, props.minRange);
        }
        else
            Log.ErrorOnce($"Trying to display turret range for {checkingDef} failed, since its turret ({(checkingDef as ThingDef)?.building?.turretGunDef.ToStringSafe()}) " +
                          $"has no valid verbs to grab the range from. Either make sure the turret has a verb with a supported class, or use {nameof(ExpandedShowTurretRadiusExtension)} " +
                          $"def mod extension to specify supported verb classes.", Gen.HashCombineInt(checkingDef?.defNameHash ?? "null".GetHashCode(), 422220065));

        return true;
    }

    private static bool IsValidVerb(VerbProperties v, ExpandedShowTurretRadiusExtension extension)
    {
        // Extension allows for some extra configuration
        if (extension != null)
        {
            // We're searching for a specific verb class
            if (extension.allowedVerbClass != null)
            {
                // If allowing any verb, search for subclasses of that verb
                if (extension.allowAnyVerb)
                    return extension.allowedVerbClass.IsAssignableFrom(v.verbClass);

                // If not allowing any verb, only allow exact match
                return extension.allowedVerbClass == v.verbClass;
            }

            // If we allow any verb, just check that the verb class is not null and is a verb.
            if (extension.allowAnyVerb)
                return typeof(Verb).IsAssignableFrom(v.verbClass);
        }

        // Vanilla check, expanded to allow any Verb_Shoot (rather than only Verb_Shoot) as well as any Verb_ShootBeam
        return typeof(Verb_Shoot).IsAssignableFrom(v.verbClass) || typeof(Verb_Spray).IsAssignableFrom(v.verbClass) || typeof(Verb_ShootBeam).IsAssignableFrom(v.verbClass);
    }
}