using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class KCSG_Skyfaller : Skyfaller
    {
        public Rot4 rot = Rot4.Invalid;

        private bool once = false;

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Thing thingForGraphic = this.GetThingForGraphic();
            float num = 0f;
            if (this.def.skyfaller.rotateGraphicTowardsDirection)
            {
                num = this.angle;
            }
            if (this.def.skyfaller.angleCurve != null)
            {
                this.angle = this.def.skyfaller.angleCurve.Evaluate(this.TimeInAnimation);
            }
            if (this.def.skyfaller.rotationCurve != null)
            {
                num += this.def.skyfaller.rotationCurve.Evaluate(this.TimeInAnimation);
            }
            if (this.def.skyfaller.xPositionCurve != null)
            {
                drawLoc.x += this.def.skyfaller.xPositionCurve.Evaluate(this.TimeInAnimation);
            }
            if (this.def.skyfaller.zPositionCurve != null)
            {
                drawLoc.z += this.def.skyfaller.zPositionCurve.Evaluate(this.TimeInAnimation);
            }
            if (rot == Rot4.South)
            {
                drawLoc.x -= 1;
                drawLoc.z -= 1;
            }
            this.Graphic.Draw(drawLoc, flip ? thingForGraphic.Rotation.Opposite : rot, thingForGraphic, num);
            this.DrawDropSpotShadow();
        }

        public void SaveImpact()
        {
            if (this.def.skyfaller.CausesExplosion)
            {
                GenExplosion.DoExplosion(base.Position, base.Map, this.def.skyfaller.explosionRadius, this.def.skyfaller.explosionDamage, null, GenMath.RoundRandom((float)this.def.skyfaller.explosionDamage.defaultDamage * this.def.skyfaller.explosionDamageFactor), -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, (!this.def.skyfaller.damageSpawnedThings) ? this.innerContainer.ToList<Thing>() : null);
            }
            this.SpawnThings();
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);

            foreach (var item in this.OccupiedRect())
            {
                if (this.Map == Find.CurrentMap && item.InBounds(this.Map))
                {
                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(KThingDefOf.KCSG_LongMote_DustPuff, null);
                    moteThrown.Scale = Rand.Range(3f, 6f);
                    moteThrown.rotationRate = (float)Rand.Range(-60, 60);
                    moteThrown.exactPosition = item.ToVector3();
                    moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
                    GenSpawn.Spawn(moteThrown, item, this.Map, WipeMode.Vanish);
                }
            }

            this.Destroy(DestroyMode.Vanish);
        }

        public override void Tick()
        {
            if (ticksToImpact <= 60 && !once)
            {
                foreach (var item in this.OccupiedRect())
                {
                    if (this.Map == Find.CurrentMap && item.InBounds(this.Map))
                    {
                        MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(KThingDefOf.KCSG_LongMote_DustPuff, null);
                        moteThrown.Scale = Rand.Range(3f, 6f);
                        moteThrown.rotationRate = (float)Rand.Range(-60, 60);
                        moteThrown.exactPosition = item.ToVector3();
                        moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
                        GenSpawn.Spawn(moteThrown, item, this.Map, WipeMode.Vanish);
                    }
                }
                once = true;
            }
            base.Tick();
        }

        protected override void SpawnThings()
        {
            int delayTicks = (int)(GenDate.TicksPerHour * Rand.Range(4f, 8f));
            for (int i = this.innerContainer.Count - 1; i >= 0; i--)
            {
                if (rot == Rot4.Invalid) rot = this.innerContainer[i].def.defaultPlacingRot;
                CompCanBeDormant compDormant = this.innerContainer[i].TryGetComp<CompCanBeDormant>();
                if (compDormant != null)
                {
                    compDormant.wokeUpTick = Find.TickManager.TicksGame + delayTicks;
                }
                GenPlace.TryPlaceThing(this.innerContainer[i], base.Position, base.Map, ThingPlaceMode.Near, delegate (Thing thing, int count)
                {
                    PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
                    if (thing.def.Fillage == FillCategory.Full && this.def.skyfaller.CausesExplosion && this.def.skyfaller.explosionDamage.isExplosive && thing.Position.InHorDistOf(base.Position, this.def.skyfaller.explosionRadius))
                    {
                        base.Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
                    }
                }, null, rot);
            }
        }

        private Thing GetThingForGraphic()
        {
            if (this.def.graphicData != null || !this.innerContainer.Any)
            {
                return this;
            }
            return this.innerContainer[0];
        }
    }
}