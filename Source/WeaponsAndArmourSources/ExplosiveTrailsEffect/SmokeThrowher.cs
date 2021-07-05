using UnityEngine;
using Verse;

namespace ExplosiveTrailsEffect
{
    public class SmokeThrowher
    {
        public static void ThrowSmokeTrail(Vector3 loc, float size, Map map, string defName)
        {
            if (GenView.ShouldSpawnMotesAt(loc, map) && !map.moteCounter.Saturated)
            {
                MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named(defName), null);
                moteThrown.Scale = Rand.Range(2f, 3f) * size;
                moteThrown.exactPosition = loc;
                moteThrown.rotationRate = Rand.Range(-0.5f, 0.5f);
                moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
                GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
            }
        }
    }
}