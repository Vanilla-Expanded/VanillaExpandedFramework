using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace VEF.AnimalBehaviours
{
    public class Hediff_Lifesteal : HediffWithComps
    {
        public int tickCounter = 0;
        public const int tickInterval = 60;

        public override void Tick()
        {
            base.Tick();
            if (tickCounter > 0)
            {
                tickCounter--;
            }
        }

        public override void Notify_PawnDamagedThing(Thing thing, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.Notify_PawnDamagedThing(thing, dinfo, result);
            Pawn otherPawn = thing as Pawn;
            Pawn pawn = this.pawn;
            if (tickCounter==0 && otherPawn != null && otherPawn.RaceProps.IsFlesh && pawn.health!=null)
            {
                List<Hediff_Injury> injuries = GetInjuries(pawn);
                if (injuries.Count > 0)
                {
                    float damageToHeal = Math.Max(8, dinfo.Amount * 0.3f);
                    Hediff_Injury injury = injuries.RandomElement();
                    injury.Heal(damageToHeal);
                    tickCounter = tickInterval;
                }
                    
            }

        }

        public List<Hediff_Injury> GetInjuries(Pawn pawn)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury hediff_Injury)
                {
                     injuries.Add(hediff_Injury);                   
                }
            }
            return injuries;
        }
    }
}
