using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class Hediff_AcidBuildup : HediffWithComps
    {
        private int tickMax = 64;
        private int tickCounter = 0;



        public override void Tick()
        {
            base.Tick();
            tickCounter++;
            if (tickCounter > tickMax)
            {
                CompAcidImmunity comp = pawn.TryGetComp<CompAcidImmunity>();
                if (comp != null)
                {
                    tickCounter = 0;
                }
                else
                {
                    pawn.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("VEF_SecondaryAcidBurn"), 1f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    tickCounter = 0;

                }

            }


        }




    }
}
