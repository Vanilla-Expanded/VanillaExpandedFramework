using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    public static class SmokeMaker
    {
        public static void ThrowMoteDef(ThingDef moteDef, Vector3 loc, Map map, float size, float velocity, float angle, float rotation)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority) return;
            var moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef);
            moteThrown.Scale = Rand.Range(1f, 2f) * size;
            moteThrown.rotationRate = rotation;
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity(angle, velocity);
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
        }

        public static void ThrowFleckDef(FleckDef fleckDef, Vector3 loc, Map map, float size, float velocity, float angle, float rotation)
        {
            var data = default(FleckCreationData);
            data.def = fleckDef;
            data.spawnPosition = loc;
            data.scale = Rand.Range(1f, 2f) * size;
            data.rotationRate = rotation;
            data.velocitySpeed = velocity;
            data.velocityAngle = angle;
            map.flecks.CreateFleck(data);
        }

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

        public static void ThrowFlintLockSmoke(Vector3 loc, Map map, float size)
        {
            if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(InternalDefOf.VEF_FlintlockSmoke, null);
            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.rotationRate = Rand.Range(-30f, 30f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
            GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
        }
    }
}
