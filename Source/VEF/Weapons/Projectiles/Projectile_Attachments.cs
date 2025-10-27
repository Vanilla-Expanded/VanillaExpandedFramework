using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    public class Projectile_Attachments : Bullet
    {
        public ProjectileExtension cachedProjectileExtension;
        private Effecter effecter;

        public ProjectileExtension CachedProjectileExtension
        {
            get
            {
                if(cachedProjectileExtension == null)
                {
                    cachedProjectileExtension=this.def.GetModExtension<ProjectileExtension>();
                }
                return cachedProjectileExtension;

            }

        }

        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(CachedProjectileExtension.fleckRefreshInterval) && CachedProjectileExtension.attachedFleck != null)
            {

                try
                {
                    FleckMaker.AttachedOverlay(this, CachedProjectileExtension.attachedFleck, Vector3.zero, CachedProjectileExtension.fleckScale, -1f);

                }
                catch (Exception) { }

            }
            if (CachedProjectileExtension.attachedEffecter != null && effecter == null)
            {
                effecter = CachedProjectileExtension.attachedEffecter.SpawnAttached(this, this.Map);
            }
            effecter?.EffectTick(this, this);
        }


    }

    public class Projectile_Attachments_Explosive : Projectile_Explosive
    {
        public ProjectileExtension cachedProjectileExtension;
        private Effecter effecter;

        public ProjectileExtension CachedProjectileExtension
        {
            get
            {
                if (cachedProjectileExtension == null)
                {
                    cachedProjectileExtension = this.def.GetModExtension<ProjectileExtension>();
                }
                return cachedProjectileExtension;

            }

        }

        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(CachedProjectileExtension.fleckRefreshInterval) && CachedProjectileExtension.attachedFleck != null)
            {
                try
                {
                    FleckMaker.AttachedOverlay(this, CachedProjectileExtension.attachedFleck, Vector3.zero, CachedProjectileExtension.fleckScale, -1f);

                }
                catch (Exception) { }

            }
            if (CachedProjectileExtension.attachedEffecter != null && effecter == null)
            {
                effecter = CachedProjectileExtension.attachedEffecter.SpawnAttached(this, this.Map);
            }
            effecter?.EffectTick(this, this);
        }


    }
}
