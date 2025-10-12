using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEF.Weapons
{
    public class WeaponTraitWorker_Extended : WeaponTraitWorker
    {

        public void Notify_Added(Thing thing)
        {
            WeaponTraitDefExtension extension = this.def.GetModExtension<WeaponTraitDefExtension>();
            if (extension?.refreshMaxHitPointsStat == true) {
                StatDefOf.MaxHitPoints.Worker.ClearCacheForThing(thing);
                thing.HitPoints = thing.MaxHitPoints;
            }
            
        }

        public void Notify_TraitRemoved()
        {

        }


    }
}
