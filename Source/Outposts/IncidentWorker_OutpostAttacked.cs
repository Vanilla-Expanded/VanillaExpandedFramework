using System.Linq;
using RimWorld;
using Verse;

namespace Outposts
{
    public class IncidentWorker_OutpostAttacked : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms) => Find.WorldObjects.AllWorldObjects.Any(wo => wo is Outpost {Faction: {IsPlayer: true}});

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!Find.WorldObjects.AllWorldObjects.OfType<Outpost>().TryRandomElement(out var target)) return false;
            var pops = target.PawnCount;
            if (pops < StorytellerUtilityPopulation.AdjustedPopulation) parms.points *= pops / StorytellerUtilityPopulation.AdjustedPopulation;
            parms.target = GetOrGenerateMapUtility.GetOrGenerateMap(target.Tile, target.def);
            return base.TryExecuteWorker(parms);
        }
    }
}