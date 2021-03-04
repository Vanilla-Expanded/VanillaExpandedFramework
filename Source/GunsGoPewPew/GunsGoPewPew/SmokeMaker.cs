using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VWEMakeshift 
{
    public static class SmokeMaker
    {
        public static void ThrowMoteDef(ThingDef moteDef, Vector3 loc, Map map, float size, float velocity, float angle, float rotation)
        {
            if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef, null);
			moteThrown.Scale = Rand.Range(1f, 2f) * size;
			moteThrown.rotationRate = rotation;
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity(angle, velocity);
			GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
        }
    }
}
