using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace AnimalBehaviours
{
    public class Hediff_AcidBuildup : HediffWithComps
    {
        private int tickMax = 64;
        private int tickCounter = 0;

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

        public override void Tick()
        {
            base.Tick();
            tickCounter++;
            if (tickCounter > tickMax)
            {             
                if (Immunity is null)
                {
                    pawn.TakeDamage(new DamageInfo(InternalDefOf.VEF_SecondaryAcidBurn, 1f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                }
                tickCounter = 0;
            }
        }
    }
}
