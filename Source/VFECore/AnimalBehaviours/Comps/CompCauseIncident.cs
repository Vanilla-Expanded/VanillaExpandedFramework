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
            if (this.parent.IsHashIntervalTick(Props.checkingInterval) && this.parent.Map != null 
                && (!Props.requiresTamed ||(Props.requiresTamed && this.parent.Faction!=null&& this.parent.Faction.IsPlayer)))
            {
                IncidentDef incidentDef = IncidentDef.Named(Props.incidentToCause);
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, this.parent.Map);
                incidentDef.Worker.TryExecute(parms);
            }
        }





    }
}

