using RimWorld;
using UnityEngine;
using Verse;

namespace OPToxic
{
    public class OPOrbitalBeam : Gas
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, true);
            destroyTick = Find.TickManager.TicksGame + def.gas.expireSeconds.RandomInRange.SecondsToTicks();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref destroyTick, "destroyTick", 0, false);
        }

        public override void Tick()
        {
            if (destroyTick <= Find.TickManager.TicksGame)
            {
                Destroy(DestroyMode.Vanish);
            }
            graphicRotation += graphicRotationSpeed;
            if (!this.DestroyedOrNull())
            {
                Map map = Map;
                IntVec3 position = Position;
                if (Find.TickManager.TicksGame % 10 == 0)
                {
                    FleckMaker.ThrowSmoke(GenThing.TrueCenter(this) + new Vector3(0f, 0f, 0.1f), map, 1f);
                }
                if (Find.TickManager.TicksGame % 300 == 0)
                {
                    OPPowerBeam oppowerBeam = (OPPowerBeam)GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("OPPowerBeam", true), position, map, WipeMode.Vanish);
                    oppowerBeam.duration = 120;
                    oppowerBeam.instigator = this;
                    oppowerBeam.weaponDef = InternalDefOf.OrbitalTargeterPowerBeam;
                    oppowerBeam.StartStrike();
                    if (!this.DestroyedOrNull())
                    {
                        Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
    }
}