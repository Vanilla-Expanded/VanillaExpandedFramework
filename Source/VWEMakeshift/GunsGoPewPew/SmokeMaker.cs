using UnityEngine;
using Verse;

namespace VWEMakeshift
{
  public static class SmokeMaker
  {
    public static void ThrowMoteDef(ThingDef moteDef, Vector3 loc, Map map, float size, float velocity, float angle, float rotation)
    {
      if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority) return;
      var moteThrown = (MoteThrown) ThingMaker.MakeThing(moteDef);
      moteThrown.Scale         = Rand.Range(1f, 2f) * size;
      moteThrown.rotationRate  = rotation;
      moteThrown.exactPosition = loc;
      moteThrown.SetVelocity(angle, velocity);
      GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
    }

    public static void ThrowFleckDef(FleckDef fleckDef, Vector3 loc, Map map, float size, float velocity, float angle, float rotation)
    {
      var data = default(FleckCreationData);
      data.def           = fleckDef;
      data.spawnPosition = loc;
      data.scale         = Rand.Range(1f, 2f) * size;
      data.rotationRate  = rotation;
      data.velocitySpeed = velocity;
      data.velocityAngle = angle;
      map.flecks.CreateFleck(data);
    }
  }
}
