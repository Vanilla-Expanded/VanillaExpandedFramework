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
    public class Hediff_VenomousAttacks : HediffWithComps
    {
        public override void Notify_PawnDamagedThing(Thing thing, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.Notify_PawnDamagedThing(thing, dinfo, result);
            Pawn pawn = thing as Pawn;
            if(pawn!=null && pawn.RaceProps.IsFlesh)
            {
                float num = 0.05f;
                num *= Mathf.Max(1f - pawn.GetStatValue(StatDefOf.ToxicResistance), 0f);
                num /= pawn.BodySize;
                if (num != 0f)
                {
                    
                    HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
                }

            }

        }
    }
}
