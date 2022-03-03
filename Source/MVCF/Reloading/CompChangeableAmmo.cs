using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Reloading
{
    public class CompChangeableAmmo : CompReloadable, IThingHolder
    {
        private readonly ThingOwner<Thing> loadedAmmo = new ThingOwner<Thing>();

        private Thing nextAmmoItem;

        public override ThingDef CurrentProjectile => nextAmmoItem?.def?.projectileWhenLoaded;

        public IEnumerable<Pair<ThingDef, Action>> AmmoOptions =>
            loadedAmmo.Select(t => new Pair<ThingDef, Action>(t.def, () => { nextAmmoItem = t; }));

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, loadedAmmo);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return loadedAmmo;
        }

        public override Thing Reload(Thing ammo)
        {
            var t = base.Reload(ammo);
            loadedAmmo.TryAddOrTransfer(t);
            if (nextAmmoItem == null) nextAmmoItem = t;
            return null;
        }

        public override void Notify_ProjectileFired()
        {
            base.Notify_ProjectileFired();
            nextAmmoItem.stackCount--;
            if (nextAmmoItem.stackCount == 0)
            {
                loadedAmmo.Remove(nextAmmoItem);
                nextAmmoItem.Destroy();
                nextAmmoItem = loadedAmmo.FirstOrFallback();
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            ShotsRemaining = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            loadedAmmo.ExposeData();
            Scribe_References.Look(ref nextAmmoItem, "nextLoadedItem");
        }

        public override void Unload()
        {
            loadedAmmo.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
            nextAmmoItem = null;
            ShotsRemaining = 0;
        }
    }
}