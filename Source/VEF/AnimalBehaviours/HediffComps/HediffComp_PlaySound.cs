using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Sound;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_PlaySound : HediffComp
    {
        private Sustainer sustainer;
        public HediffCompProperties_PlaySound Props => (HediffCompProperties_PlaySound)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Props.sustainer != null)
            {
                if (sustainer == null || sustainer.Ended)
                {
                    sustainer = Props.sustainer.TrySpawnSustainer(SoundInfo.InMap(Pawn, MaintenanceType.PerTick));
                }
                sustainer.Maintain();
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.sustainer != null)
            {
                if (!sustainer.Ended)
                {
                    sustainer?.End();
                }
            }
            if (Props.endSound != null)
            {
                Props.endSound.PlayOneShot(base.Pawn);
            }
        }
    }
}
