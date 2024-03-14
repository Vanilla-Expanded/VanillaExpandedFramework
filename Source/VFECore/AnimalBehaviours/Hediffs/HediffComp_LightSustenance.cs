
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_LightSustenance : HediffComp
    {

        public float growOptimalGlow = 0.4f;
        private bool addHediffOnce = true;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref this.addHediffOnce, "addHediffOnce", true, false);
        }

       
        public HediffCompProperties_LightSustenance Props
        {
            get
            {
                return (HediffCompProperties_LightSustenance)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.parent.pawn;

            if (pawn.Spawned)
            {
                if (addHediffOnce)
                {
                    pawn.health.AddHediff(InternalDefOf.VEF_LightSustenance);
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(InternalDefOf.VEF_LightSustenance, false);
                    hediff.Severity = 0.2f;
                    addHediffOnce = false;
                }
                float num = pawn.Map.glowGrid.GroundGlowAt(pawn.Position, false);


                if (num >= growOptimalGlow)
                {

                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(InternalDefOf.VEF_LightSustenance, false);

                    if ((hediff != null) && hediff.Severity > 0f)
                    {
                        hediff.Severity -= 0.000010f;

                    }
                }
                else
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(InternalDefOf.VEF_LightSustenance, false);

                    if ((hediff != null) && hediff.Severity < 1f)
                    {

                        hediff.Severity += 0.000010f;


                    }
                }
            }
        }


    }
}
