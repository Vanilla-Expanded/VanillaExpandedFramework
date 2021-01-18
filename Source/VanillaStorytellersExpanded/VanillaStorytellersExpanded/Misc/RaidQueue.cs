using RimWorld;
using System;
using Verse;

namespace VanillaStorytellersExpanded
{
    public class RaidQueue : IExposable
    {
        public IncidentDef incidentDef;
        public IncidentParms parms;
        public int tickToFire;
        public RaidQueue()
        {

        }

        public RaidQueue(IncidentDef incidentDef, IncidentParms parms, int tickToFire)
        {
            this.incidentDef = incidentDef;
            this.parms = parms;
            this.tickToFire = tickToFire;
        }
        public void ExposeData()
        {
            Scribe_Defs.Look(ref incidentDef, "incidentDef");
            Scribe_Deep.Look(ref parms, "parms");
            Scribe_Values.Look(ref tickToFire, "tickToFire");
        }
    }
}
