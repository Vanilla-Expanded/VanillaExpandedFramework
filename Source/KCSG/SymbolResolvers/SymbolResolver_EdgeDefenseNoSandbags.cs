using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class SymbolResolver_EdgeDefenseNoSandbags : SymbolResolver
    {
        private const int DefaultCellsPerTurret = 30;
        private const int DefaultCellsPerMortar = 75;

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            int guardsCount = rp.edgeDefenseGuardsCount ?? 0;
            int width = rp.edgeDefenseWidth ?? (!rp.edgeDefenseMortarsCount.HasValue || rp.edgeDefenseMortarsCount.Value <= 0 ? (Rand.Bool ? 2 : 4) : 4);
            width = Mathf.Clamp(width, 1, Mathf.Min(rp.rect.Width, rp.rect.Height) / 2);

            int turretCount;
            int mortarCount;
            bool turretIS = true, mortarIS = true;

            switch (width)
            {
                case 1:
                    turretCount = rp.edgeDefenseTurretsCount ?? 0;
                    mortarCount = 0;
                    break;
                case 2:
                    turretCount = rp.edgeDefenseTurretsCount ?? rp.rect.EdgeCellsCount / DefaultCellsPerTurret;
                    mortarCount = 0;
                    turretIS = false;
                    break;
                case 3:
                    turretCount = rp.edgeDefenseTurretsCount ?? rp.rect.EdgeCellsCount / DefaultCellsPerTurret;
                    mortarCount = rp.edgeDefenseMortarsCount ?? rp.rect.EdgeCellsCount / DefaultCellsPerMortar;
                    turretIS = false;
                    break;
                default:
                    turretCount = rp.edgeDefenseTurretsCount ?? rp.rect.EdgeCellsCount / DefaultCellsPerTurret;
                    mortarCount = rp.edgeDefenseMortarsCount ?? rp.rect.EdgeCellsCount / DefaultCellsPerMortar;
                    turretIS = false;
                    mortarIS = false;
                    break;
            }

            if (faction != null && faction.def.techLevel < TechLevel.Industrial)
            {
                turretCount = 0;
                mortarCount = 0;
            }

            if (guardsCount > 0)
            {
                Lord lord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map);
                for (int index1 = 0; index1 < guardsCount; ++index1)
                {
                    PawnGenerationRequest generationRequest = new PawnGenerationRequest(faction.RandomPawnKind(), faction, mustBeCapableOfViolence: true);
                    ResolveParams rpGuard = rp;
                    rpGuard.faction = faction;
                    rpGuard.singlePawnLord = lord;
                    rpGuard.singlePawnGenerationRequest = new PawnGenerationRequest?(generationRequest);

                    rpGuard.singlePawnSpawnCellExtraPredicate = rpGuard.singlePawnSpawnCellExtraPredicate ?? (x =>
                    {
                        CellRect cellRect = rp.rect;
                        for (int index2 = 0; index2 < width && !cellRect.IsOnEdge(x); ++index2)
                            cellRect = cellRect.ContractedBy(1);
                        return true;
                    });
                    BaseGen.symbolStack.Push("pawn", rpGuard);
                }
            }

            CellRect mortarRect = mortarIS ? rp.rect : rp.rect.ContractedBy(1);
            for (int index = 0; index < mortarCount; ++index)
            {
                ResolveParams rpMortar = rp;
                rpMortar.faction = faction;
                rpMortar.rect = mortarRect;

                BaseGen.symbolStack.Push("edgeMannedMortar", rpMortar);
            }

            CellRect turretRect = turretIS ? rp.rect : rp.rect.ContractedBy(1);
            for (int index = 0; index < turretCount; ++index)
            {
                ResolveParams rpTurret = rp;
                rpTurret.faction = faction;
                rpTurret.singleThingDef = ThingDefOf.Turret_MiniTurret;
                rpTurret.rect = turretRect;
                rpTurret.edgeThingAvoidOtherEdgeThings = new bool?(rp.edgeThingMustReachMapEdge ?? true);

                BaseGen.symbolStack.Push("edgeThing", rpTurret);
            }
        }
    }
}
