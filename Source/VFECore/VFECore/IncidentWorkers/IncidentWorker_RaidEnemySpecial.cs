using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace VFECore
{

    public class IncidentWorker_RaidEnemySpecial : IncidentWorker_RaidEnemy
    {

        private IncidentDefExtension IncidentDefExtension => IncidentDefExtension.Get(def);

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            if (IncidentDefExtension.forcedFaction == null)
                return base.TryResolveRaidFaction(parms);

            parms.faction = Find.FactionManager.FirstFactionOfDef(IncidentDefExtension.forcedFaction);
            return true;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (IncidentDefExtension.forcedPointsRange == IntRange.one)
                base.ResolveRaidPoints(parms);

            else
                parms.points = IncidentDefExtension.forcedPointsRange.RandomInRange * Find.Storyteller.difficulty.threatScale;
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (IncidentDefExtension.forcedStrategy == null)
                base.ResolveRaidStrategy(parms, groupKind);

            else
                parms.raidStrategy = IncidentDefExtension.forcedStrategy;
        }

    }

}
