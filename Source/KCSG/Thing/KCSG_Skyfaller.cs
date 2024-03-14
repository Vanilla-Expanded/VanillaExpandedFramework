using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class KCSG_Skyfaller : Skyfaller
    {
        public Rot4 rot = Rot4.Invalid;



        private bool once = false;

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Thing thingForGraphic = GetThingForGraphic();
            float num = 0f;
            if (def.skyfaller.rotateGraphicTowardsDirection)
            {
                num = angle;
            }
            if (def.skyfaller.angleCurve != null)
            {
                angle = def.skyfaller.angleCurve.Evaluate(TimeInAnimation);
            }
            if (def.skyfaller.rotationCurve != null)
            {
                num += def.skyfaller.rotationCurve.Evaluate(TimeInAnimation);
            }
            if (def.skyfaller.xPositionCurve != null)
            {
                drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
            }
            if (def.skyfaller.zPositionCurve != null)
            {
                drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
            }
            if (rot == Rot4.South)
            {
                drawLoc.x -= 1;
                drawLoc.z -= 1;
            }
            Graphic.Draw(drawLoc, flip ? thingForGraphic.Rotation.Opposite : rot, thingForGraphic, num);
            DrawDropSpotShadow();
        }

        public void SaveImpact()
        {
            if (def.skyfaller.CausesExplosion)
            {
                GenExplosion.DoExplosion(Position, Map, def.skyfaller.explosionRadius, def.skyfaller.explosionDamage, null, GenMath.RoundRandom(def.skyfaller.explosionDamage.defaultDamage * def.skyfaller.explosionDamageFactor));
            }
            SpawnThings();
            innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);

            foreach (var item in this.OccupiedRect())
            {
                if (Map == Find.CurrentMap && item.InBounds(Map))
                {
                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(AllDefOf.KCSG_LongMote_DustPuff, null);
                    moteThrown.Scale = Rand.Range(3f, 6f);
                    moteThrown.rotationRate = Rand.Range(-60, 60);
                    moteThrown.exactPosition = item.ToVector3();
                    moteThrown.SetVelocity(Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
                    GenSpawn.Spawn(moteThrown, item, Map, WipeMode.Vanish);
                }
            }

            Destroy(DestroyMode.Vanish);
        }

        public override void Tick()
        {
            if (ticksToImpact <= 60 && !once)
            {
                foreach (var item in this.OccupiedRect())
                {
                    if (Map == Find.CurrentMap && item.InBounds(Map))
                    {
                        MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(AllDefOf.KCSG_LongMote_DustPuff, null);
                        moteThrown.Scale = Rand.Range(3f, 6f);
                        moteThrown.rotationRate = Rand.Range(-60, 60);
                        moteThrown.exactPosition = item.ToVector3();
                        moteThrown.SetVelocity(Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
                        GenSpawn.Spawn(moteThrown, item, Map, WipeMode.Vanish);
                    }
                }
                once = true;
            }
            base.Tick();
        }

        protected override void SpawnThings()
        {
            int delayTicks = (int)(GenDate.TicksPerHour * Rand.Range(4f, 8f));
            for (int i = innerContainer.Count - 1; i >= 0; i--)
            {
                if (rot == Rot4.Invalid) rot = innerContainer[i].def.defaultPlacingRot;
                CompCanBeDormant compDormant = innerContainer[i].TryGetComp<CompCanBeDormant>();
                if (compDormant != null)
                {
                    compDormant.wokeUpTick = Find.TickManager.TicksGame + delayTicks;
                    
                }
                GenPlace.TryPlaceThing(innerContainer[i], Position, Map, ThingPlaceMode.Direct, delegate (Thing thing, int count)
                {
                    PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
                    if (thing.def.Fillage == FillCategory.Full && def.skyfaller.CausesExplosion && def.skyfaller.explosionDamage.isExplosive && thing.Position.InHorDistOf(Position, def.skyfaller.explosionRadius))
                    {
                        Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
                    }
                }, null, rot);
            }
        }

        private Thing GetThingForGraphic()
        {
            if (def.graphicData != null || !innerContainer.Any)
            {
                return this;
            }
            return innerContainer[0];
        }
    }
}