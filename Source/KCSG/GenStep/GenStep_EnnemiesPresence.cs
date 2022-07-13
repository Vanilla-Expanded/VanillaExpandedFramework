using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
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
            Faction fac = forcedfaction != null ? Find.FactionManager.FirstFactionOfDef(forcedfaction) : parms.sitePart.site.Faction;

            if (fac == null)
            {
                Find.FactionManager.RandomEnemyFaction(minTechLevel: TechLevel.Neolithic);
                parms.sitePart.site.SetFaction(fac);
            }
            else
            {
                parms.sitePart.site.SetFaction(fac);
            }

            Lord defend = LordMaker.MakeNewLord(fac, new LordJob_DefendBase(fac, map.Center), map);

            foreach (var pawn in GeneratePawns(map, fac, parms))
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
                defend.AddPawn(pawn);
            }
        }

        private IEnumerable<Pawn> GeneratePawns(Map map, Faction faction, GenStepParams parms)
        {
            float p;
            if (parms.sitePart?.parms != null && parms.sitePart.parms.threatPoints >= defaultPointsRange.min && parms.sitePart.parms.threatPoints <= defaultPointsRange.max)
            {
                p = parms.sitePart.parms.threatPoints;
                Debug.Message($"Using sitePart parms threat points: {p}");
            }
            else
            {
                p = defaultPointsRange.RandomInRange;
                Debug.Message($"Using in-range threat points: {p}. Choosen from {defaultPointsRange}");
            }
            p = Math.Max(p, 150) * pointMultiplier;
            Debug.Message($"Final threat points: {p}");

            return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = faction,
                points = p
            }, true);
        }
    }
}
