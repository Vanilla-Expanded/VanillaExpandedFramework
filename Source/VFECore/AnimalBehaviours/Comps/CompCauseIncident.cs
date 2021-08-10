using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace AnimalBehaviours
{
    public class CompCauseIncident : ThingComp
    {

        public bool waitingForNight = false;
        public int checkingForNightInterval = 100;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.waitingForNight, "waitingForNight", false, false);
        }

        public CompProperties_CauseIncident Props
        {
            get
            {
                return (CompProperties_CauseIncident)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!waitingForNight && this.parent.IsHashIntervalTick(Props.checkingInterval) && this.parent.Map != null
                && (!Props.requiresTamed || (Props.requiresTamed && this.parent.Faction != null && this.parent.Faction.IsPlayer)))
            {
                IncidentDef incidentDef = IncidentDef.Named(Props.incidentToCause);

                if (incidentDef.defName == "Aurora") {
                    waitingForNight = true;
                } else {
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, this.parent.Map);
                    incidentDef.Worker.TryExecute(parms);
                }
                
            }
            if (waitingForNight && this.parent.IsHashIntervalTick(this.checkingForNightInterval) && this.parent.Map != null && GenCelestial.CurCelestialSunGlow(this.parent.Map) <= 0.4f
                && (!Props.requiresTamed || (Props.requiresTamed && this.parent.Faction != null && this.parent.Faction.IsPlayer)))
            {
                IncidentDef incidentDef = IncidentDef.Named(Props.incidentToCause);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, this.parent.Map);
                incidentDef.Worker.TryExecute(parms);
                waitingForNight = false;
            }
        }





    }
}

