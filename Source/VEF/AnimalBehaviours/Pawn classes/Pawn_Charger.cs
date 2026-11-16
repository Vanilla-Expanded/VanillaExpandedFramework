using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VEF.AnimalBehaviours
{
    public class Pawn_Charger : PawnFlyer
    {
        private int tickCounter = 0;
        private static List<Thing> tmpThings = new List<Thing>();
        private Effecter effecter;
        public int damageMultiplierInCharge = 1;

        protected override void Tick()
        {
            base.Tick();
            if (Spawned)
            {
                if (effecter == null)
                {
                    effecter = InternalDefOf.Berserk.SpawnAttached(this, Map);
                }
                effecter?.EffectTick(this, this);

                tickCounter++;
                if (tickCounter > 10)
                {
                    this.DamageCloseThings();
                    tickCounter = 0;
                }
            }
        }

        private void DamageCloseThings()
        {
            int num = GenRadial.NumCellsInRadius(3f);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = this.Position + GenRadial.RadialPattern[i];
                if (Map != null)
                {
                    if (intVec.InBounds(Map) && !this.CellImmuneToDamage(intVec))
                    {
                        Pawn firstPawn = intVec.GetFirstPawn(Map);
                        if (firstPawn == null || !firstPawn.Downed || !Rand.Bool)
                        {
                            float damageFactor = GenMath.LerpDouble(0f, 4.2f, 1f, 0.2f, intVec.DistanceTo(Position));
                            this.DoDamage(intVec, damageFactor);
                        }
                    }
                }
            }
        }


        private bool CellImmuneToDamage(IntVec3 c)
        {
            if (c.Roofed(Map) && c.GetRoof(Map).isThickRoof)
            {
                return true;
            }
            Building edifice = c.GetEdifice(Map);
            return edifice != null && edifice.def.category == ThingCategory.Building && (edifice.def.building.isNaturalRock || (edifice.def == ThingDefOf.Wall && edifice.Faction == null));
        }

        private float AverageDamage(Pawn pawn)
        {
            float averageDamage = pawn.Tools.NullOrEmpty() ? 0f : pawn.Tools.Average(x => x.power);

            return averageDamage * damageMultiplierInCharge;

        }

        private void DoDamage(IntVec3 c, float damageFactor)
        {
            tmpThings.Clear();
            tmpThings.AddRange(c.GetThingList(Map));
            Vector3 vector = c.ToVector3Shifted();
            Vector2 b = new Vector2(vector.x, vector.z);

            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (tmpThings[i] != this)
                {
                    BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                    switch (tmpThings[i].def.category)
                    {
                        case ThingCategory.Pawn:
                            {
                                Pawn pawn = (Pawn)tmpThings[i];
                                battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, InternalDefOf.VEF_DamageEvent_Charge, null);
                                Find.BattleLog.Add(battleLogEntry_DamageTaken);
                                if (pawn.RaceProps.baseHealthScale < 1f)
                                {
                                    damageFactor *= pawn.RaceProps.baseHealthScale;
                                }
                                if (pawn.RaceProps.Animal)
                                {
                                    damageFactor *= 0.75f;
                                }
                                if (pawn.Downed)
                                {
                                    damageFactor *= 0.1f;
                                }
                                break;
                            }
                        case ThingCategory.Item:
                            damageFactor *= 0.03f;
                            break;
                        case ThingCategory.Building:
                            damageFactor *= 1f;
                            break;
                        case ThingCategory.Plant:
                            damageFactor *= 1f;
                            break;
                    }
                    float amount = Mathf.Max(AverageDamage(this.FlyingPawn) * damageFactor, 1);
                    Thing thingtoHurt = tmpThings[i];
                    DamageDef damage = DamageDefOf.Blunt;

                    thingtoHurt.TakeDamage(new DamageInfo(damage, amount, 0f, 0, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null)).AssociateWithLog(battleLogEntry_DamageTaken);


                }
            }
            tmpThings.Clear();
        }

    }


}











