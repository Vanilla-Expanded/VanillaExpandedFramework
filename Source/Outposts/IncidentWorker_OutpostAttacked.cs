using System.Linq;
using RimWorld;
using Verse;

namespace Outposts
{
    public class IncidentWorker_OutpostAttacked : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms) => Find.WorldObjects.AllWorldObjects.Any(wo => wo is Outpost {Faction: {IsPlayer: true}});

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!Find.WorldObjects.AllWorldObjects.OfType<Outpost>().TryRandomElement(out var target)) return false;
            parms.target = GetOrGenerateMapUtility.GetOrGenerateMap(target.Tile, new IntVec3(150, 1, 150), target.def);
            parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            return base.TryExecuteWorker(parms);
        }
    }
}