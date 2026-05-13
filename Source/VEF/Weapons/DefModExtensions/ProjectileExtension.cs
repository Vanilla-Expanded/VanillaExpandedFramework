using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    public class ProjectileExtension : DefModExtension
    {
        public int      beamLifetimeTicks   = 30;
        public int      beamSkyFadeInTicks  = 0;
        public int      beakSkyHoldTikcs    = 25;
        public int      beakSkyFadeOutTicks = 5;
        public float    flashIntensity      = -1f;
        public FleckDef hitFleck;
        public bool excludeFromStaticCollection = false;
        public EffecterDef attachedEffecter;
        public FleckDef attachedFleck;
        public float fleckScale = 1;
        public int fleckRefreshInterval = 10;

        /// <summary>
        /// A filth that will be spawned if the projectile misses without hitting anything (and isn't intercepted by, for example, shields)
        /// </summary>
        public ThingDef filthOnMiss;
        /// <summary>
        /// A chance that <see cref="filthOnMiss"/> will be spawned.
        /// </summary>
        public float filthOnMissChance;
        /// <summary>
        /// The amount of <see cref="filthOnMiss"/> to spawn.
        /// </summary>
        public IntRange filthOnMissCount;
    }
}
