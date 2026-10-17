using RimWorld;
using Verse;

namespace VEF.Things;

public class CustomFireSpark : Projectile
{
    public ThingDef fireDef;

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var prevMap = Map;
        base.Impact(hitThing, blockedByShield);
        var instigator = launcher switch
        {
            CustomFire customFire => customFire.instigator,
            // Probably can't happen, but may as well keep it here in case anyone ever needs it?
            Fire fire => fire.instigator,
            _ => launcher
        };

        if (fireDef is CustomFireDef customFireDef)
            CustomFireUtility.TryStartCustomFireIn(Position, prevMap, CustomFire.MinFireSize, instigator, customFireDef);
        // Supports spreading normal fire as well
        else if (fireDef == ThingDefOf.Fire)
            FireUtility.TryStartFireIn(Position, prevMap, Fire.MinFireSize, instigator);
    }
}