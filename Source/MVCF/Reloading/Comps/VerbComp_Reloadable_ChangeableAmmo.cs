using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.VerbComps;
using Verse;

namespace MVCF.Reloading.Comps;

public class VerbComp_Reloadable_ChangeableAmmo : VerbComp_Reloadable, IThingHolder
{
    private readonly ThingOwner<Thing> loadedAmmo = new();

    private Thing nextAmmoItem;

    public virtual IEnumerable<Pair<ThingDef, Action>> AmmoOptions => loadedAmmo.Select(t => new Pair<ThingDef, Action>(t.def, () => { nextAmmoItem = t; }));

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, loadedAmmo);
    }

    public ThingOwner GetDirectlyHeldThings() => loadedAmmo;
    public IThingHolder ParentHolder => null;
    public override ThingDef ProjectileOverride(ThingDef oldProjectile) => nextAmmoItem?.def.projectileWhenLoaded;

    public override void ExposeData()
    {
        base.ExposeData();
        loadedAmmo.ExposeData();
        Scribe_References.Look(ref nextAmmoItem, "nextLoadedItem");
    }

    public override Thing Reload(Thing ammo)
    {
        var t = base.Reload(ammo);
        loadedAmmo.TryAddOrTransfer(t);
        nextAmmoItem ??= t;
        return null;
    }

    public override void Initialize(VerbCompProperties props)
    {
        base.Initialize(props);
        ShotsRemaining = 0;
    }

    public override void Notify_ShotFired()
    {
        base.Notify_ShotFired();
        nextAmmoItem.stackCount--;
        if (nextAmmoItem.stackCount == 0)
        {
            loadedAmmo.Remove(nextAmmoItem);
            nextAmmoItem.Destroy();
            nextAmmoItem = loadedAmmo.FirstOrFallback();
        }
    }

    public override void Unload()
    {
        loadedAmmo.TryDropAll(parent.Verb.caster.Position, parent.Verb.caster.Map, ThingPlaceMode.Near);
        nextAmmoItem = null;
        ShotsRemaining = 0;
    }
}
