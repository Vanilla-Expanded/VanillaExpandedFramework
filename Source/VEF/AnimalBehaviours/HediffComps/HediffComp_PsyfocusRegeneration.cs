
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_PsyfocusRegeneration : HediffComp
    {
        public HediffCompProperties_PsyfocusRegeneration Props
        {
            get
            {
                return (HediffCompProperties_PsyfocusRegeneration)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Pawn.IsHashIntervalTick(Props.rateInTicks, delta))
            {
                Pawn pawn = parent.pawn;

                if (pawn.psychicEntropy != null)
                {
                    pawn.psychicEntropy.OffsetPsyfocusDirectly(Props.regenAmount);
                  
                }
            }

        }

       
    }
}
