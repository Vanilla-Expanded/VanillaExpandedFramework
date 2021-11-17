using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace KCSG
{
    public class GenStep_EnnemiesPresence : GenStep
    {
		public FactionDef forcedfaction;
		public float pointMultiplier = 1f;
		public bool spawnOnEdge = false;
		public FloatRange defaultPointsRange = new FloatRange(300f, 500f);

		public override int SeedPart
		{
			get
			{
				return 1466666193;
			}
		}


		public override void Generate(Map map, GenStepParams parms)
		{
			Faction fac = this.forcedfaction != null ? Find.FactionManager.FirstFactionOfDef(this.forcedfaction) : Find.FactionManager.RandomEnemyFaction(minTechLevel: TechLevel.Neolithic);
			parms.sitePart.site.SetFaction(fac);

			IEnumerable<Pawn> pawns = this.GeneratePawns(map, fac, parms);
			
			foreach (Pawn pawn in pawns)
			{
				IntVec3 loc;
				if (spawnOnEdge)
				{
					if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachColony(x), map, CellFinder.EdgeRoadChance_Ignore, out loc))
					{
						pawn.Discard();
						break;
					}
				}
				else if (!CellFinder.TryFindRandomSpawnCellForPawnNear(map.Center, map, out loc, 2))
				{
					pawn.Discard();
					break;
				}
				GenSpawn.Spawn(pawn, loc, map);
			}

			if (!pawns.Any())
				return;
		
			if (!this.spawnOnEdge)
			{
				LordMaker.MakeNewLord(fac, new LordJob_DefendBase(fac, map.Center), map, pawns);
			}
            else
            {
				LordMaker.MakeNewLord(fac, new LordJob_AssaultColony(fac, canTimeoutOrFlee: true, canPickUpOpportunisticWeapons: true), map, pawns);
			}
		}

		private IEnumerable<Pawn> GeneratePawns(Map map, Faction faction, GenStepParams parms)
        {
			float p = (parms.sitePart != null ? parms.sitePart.parms.threatPoints : this.defaultPointsRange.RandomInRange) * this.pointMultiplier;
			return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Combat,
				tile = map.Tile,
				faction = faction,
                points = Math.Max(p, defaultPointsRange.min)
            }, true);
		}
	}
}
