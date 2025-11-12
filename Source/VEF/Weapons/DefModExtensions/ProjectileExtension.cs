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
    }
}
