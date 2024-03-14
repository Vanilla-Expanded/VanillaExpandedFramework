using RimWorld;
using UnityEngine;
using Verse;

namespace OPToxic
{
    public class OPOrbitalBomb : Gas
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
                    OPBombardment opbombardment = (OPBombardment)GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("OPBombardment", true), position, map, WipeMode.Vanish);
                    opbombardment.duration = 120;
                    opbombardment.instigator = this;
                    opbombardment.weaponDef = InternalDefOf.OrbitalTargeterBombardment;
                    opbombardment.StartStrike();
                    if (!this.DestroyedOrNull())
                    {
                        Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
    }
}