using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace AnimalBehaviours
{
    public class HediffComp_CauseIncident : HediffComp
    {

        public bool waitingForNight = false;
        public int checkingForNightInterval = 100;
        

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref this.waitingForNight, "waitingForNight", false, false);
        }

        public HediffCompProperties_CauseIncident Props
        {
            get
            {
                return (HediffCompProperties_CauseIncident)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!waitingForNight && this.parent.pawn.IsHashIntervalTick(Props.checkingInterval) && this.parent.pawn.Map != null
                && (!Props.requiresTamed || (Props.requiresTamed && this.parent.pawn.Faction != null && this.parent.pawn.Faction.IsPlayer)))
            {
                IncidentDef incidentDef = IncidentDef.Named(Props.incidentToCause);

                if (incidentDef.defName == "Aurora")
                {
                    waitingForNight = true;
                }
                else
                {
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, this.parent.pawn.Map);
                    incidentDef.Worker.TryExecute(parms);
                }

            }
            if (waitingForNight && this.parent.pawn.IsHashIntervalTick(this.checkingForNightInterval) && this.parent.pawn.Map != null && GenCelestial.CurCelestialSunGlow(this.parent.pawn.Map) <= 0.4f
                && (!Props.requiresTamed || (Props.requiresTamed && this.parent.pawn.Faction != null && this.parent.pawn.Faction.IsPlayer)))
            {
                IncidentDef incidentDef = IncidentDef.Named(Props.incidentToCause);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, this.parent.pawn.Map);
                incidentDef.Worker.TryExecute(parms);
                waitingForNight = false;
            }
        }





    }
}

