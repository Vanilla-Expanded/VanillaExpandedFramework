using System;
using UnityEngine;
using Verse;

namespace SmokingGun
{
	public static class MoteMaker2
	{
		public static void ThrowFlintLockSmoke(Vector3 loc, Map map, float size)
		{
			if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(GrimworldMoteDefOf.Grimworld_FlintlockSmoke, null);
			moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
			moteThrown.rotationRate = Rand.Range(-30f, 30f);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
			GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
		}
	}
}
