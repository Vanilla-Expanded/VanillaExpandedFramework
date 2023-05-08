using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MVCF.Comps;
using MVCF.VerbComps;
using Verse;

namespace MVCF.Reloading.Comps;

public class VerbComp_Reloadable_ChangeableAmmo : VerbComp_Reloadable, IThingHolder
{
    public readonly ThingOwner<Thing> LoadedAmmo = new();

    public Thing NextAmmoItem;

    public virtual IEnumerable<Pair<ThingDef, Action>> AmmoOptions => LoadedAmmo.Select(t => new Pair<ThingDef, Action>(t.def, () => { NextAmmoItem = t; }));

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, LoadedAmmo);
    }

    public ThingOwner GetDirectlyHeldThings() => LoadedAmmo;
    public IThingHolder ParentHolder => null;

    public override ThingDef ProjectileOverride(ThingDef oldProjectile)
    {
        if (NextAmmoItem == null) return null;
        if (props is VerbCompProperties_Reloadable_ChangeableAmmo { AmmoProjectileDictionary: { } ammoProjectiles }
         && ammoProjectiles.TryGetValue(NextAmmoItem.def, out var newProjectile))
            return newProjectile;

        return NextAmmoItem.def.projectileWhenLoaded;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        LoadedAmmo.ExposeData();
        Scribe_References.Look(ref NextAmmoItem, "nextLoadedItem");
    }

    public override Thing Reload(Thing ammo)
    {
        var t = base.Reload(ammo);
        LoadedAmmo.TryAddOrTransfer(t);
        NextAmmoItem ??= t;
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
        NextAmmoItem.stackCount--;
        if (NextAmmoItem.stackCount == 0)
        {
            LoadedAmmo.Remove(NextAmmoItem);
            NextAmmoItem.Destroy();
            NextAmmoItem = LoadedAmmo.FirstOrFallback();
        }
    }

    public override void Unload()
    {
        LoadedAmmo.TryDropAll(parent.Verb.caster.Position, parent.Verb.caster.Map, ThingPlaceMode.Near);
        NextAmmoItem = null;
        ShotsRemaining = 0;
    }
}

public class VerbCompProperties_Reloadable_ChangeableAmmo : VerbCompProperties_Reloadable
{
    public Dictionary<ThingDef, ThingDef> AmmoProjectileDictionary;
    public List<AmmoProjectileData> AmmoProjectiles;

    public override void PostLoadSpecial(VerbProperties verbProps, AdditionalVerbProps additionalProps, Def parentDef)
    {
        base.PostLoadSpecial(verbProps, additionalProps, parentDef);
        AmmoProjectileDictionary = AmmoProjectiles?.ToDictionary(ap => ap.Ammo, ap => ap.Projectile);
    }
}

public class AmmoProjectileData
{
    public ThingDef Ammo;
    public ThingDef Projectile;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "Ammo", xmlRoot);
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "Projectile", xmlRoot.FirstChild.Value);
    }
}
