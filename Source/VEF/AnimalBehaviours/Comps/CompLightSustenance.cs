using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class CompLightSustenance : ThingComp
    {


        public float growOptimalGlow = 0.4f;
        private bool addHediffOnce = true;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.addHediffOnce, "addHediffOnce", true);

        }


        public CompProperties_LightSustenance Props
        {
            get
            {
                return (CompProperties_LightSustenance)this.props;
            }
        }


        public override void CompTickInterval(int delta)
        {
            Pawn pawn = this.parent as Pawn;

            if (pawn.Spawned)
            {
                if (addHediffOnce)
                {
                    pawn.health.AddHediff(InternalDefOf.VEF_LightSustenance);
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(InternalDefOf.VEF_LightSustenance, false);
                    hediff.Severity = 0.2f;
                    addHediffOnce = false;
                }
                float num = this.parent.Map.glowGrid.GroundGlowAt(this.parent.Position, false);
               

                if (num >= growOptimalGlow)
                {

                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(InternalDefOf.VEF_LightSustenance, false);

                    if ((hediff != null) && hediff.Severity > 0f)
                    {
                        hediff.Severity -= 0.000010f * delta;
                       
                    }
                }
                else
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(InternalDefOf.VEF_LightSustenance, false);

                    if ((hediff != null) && hediff.Severity < 1f)
                    {

                        hediff.Severity += 0.000010f * delta;
                      

                    }
                }
            }
        }


    }
}

