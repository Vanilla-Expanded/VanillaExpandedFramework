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
    public class Hediff_AcidBuildup : HediffWithComps
    {
        private int tickMax = 65;

        public CompAcidImmunity comp;

        public CompAcidImmunity Immunity
        {
            get
            {
                if (comp == null)
                {
                    comp = pawn.TryGetComp<CompAcidImmunity>();
                }
                return comp;
            }
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(tickMax, delta))
            {             
                if (Immunity is null)
                {
                    pawn.TakeDamage(new DamageInfo(InternalDefOf.VEF_SecondaryAcidBurn, 1f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                }
            }
        }
    }
}
