using RimWorld;
using UnityEngine;
using Verse;

namespace VEF;

public class VefFleckMaker
{
    public static void MakeLightningGlow(Map map, Vector3 effectPos, float angle, float speed, float scale)
    {
        map.flecks.CreateFleck(new FleckCreationData
                                           {
                                               def               = FleckDefOf.LightningGlow,
                                               spawnPosition     = effectPos,
                                               scale             = scale,
                                               ageTicksOverride  = -1,
                                               rotationRate      = 0,
                                               velocityAngle     = angle,
                                               velocitySpeed     = speed,
                                               solidTimeOverride = 0f
                                           });
    }

    public static void MakeGaussDistortion(Map map, Vector3 effectPos, float angle, float speed, float scale)
    {
        FleckCreationData data = FleckMaker.GetDataStatic(effectPos, map, VEFDefOf.VEF_GaussDistortion, scale);
        data.rotationRate  = 90f;
        data.velocityAngle = angle + Rand.Range(-15f, 15f);
        data.velocitySpeed = speed;
        map.flecks.CreateFleck(data);
    }
}