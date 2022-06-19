using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class SymbolResolver_Settlement : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            rp.faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);

            if (GenOption.ext.useStructureLayout)
            {
                // Add hostile pawns
                AddHostilePawnGroup(rp.faction, map, rp);
                // Generate structure
                BaseGen.symbolStack.Push("kcsg_roomsgenfromstructure", rp, null);
            }
            else
            {
                // Add hostile pawns
                AddHostilePawnGroup(rp.faction, map, rp);

                // Props scatterer
                BaseGen.symbolStack.Push("kcsg_scatterpropsaround", rp, null);

                // Handle power
                BaseGen.symbolStack.Push("kcsg_settlementpower", rp, null);

                // Handle road
                BaseGen.symbolStack.Push("kcsg_generateroad", rp, null);

                // Add vanilla settlement defense
                if (GenOption.sld.vanillaLikeDefense)
                {
                    int dWidth = Rand.Bool ? 2 : 4;
                    ResolveParams edgeParms = rp;
                    edgeParms.rect = new CellRect(rp.rect.minX - dWidth, rp.rect.minZ - dWidth, rp.rect.Width + (dWidth * 2), rp.rect.Height + (dWidth * 2));
                    edgeParms.faction = rp.faction;
                    edgeParms.edgeDefenseWidth = dWidth;
                    edgeParms.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
                    BaseGen.symbolStack.Push("edgeDefense", edgeParms, null);
                }
                // Add vanilla settlement defense
                else if (GenOption.sld.vanillaLikeDefenseNoSandBags)
                {
                    int dWidth = Rand.Bool ? 2 : 4;
                    ResolveParams edgeParms = rp;
                    edgeParms.rect = new CellRect(rp.rect.minX - dWidth, rp.rect.minZ - dWidth, rp.rect.Width + (dWidth * 2), rp.rect.Height + (dWidth * 2));
                    edgeParms.faction = rp.faction;
                    edgeParms.edgeDefenseWidth = dWidth;
                    edgeParms.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
                    BaseGen.symbolStack.Push("kcsg_edgeDefense", edgeParms, null);
                }

                // Add pad if needed
                if (GenOption.sld.addLandingPad && ModLister.RoyaltyInstalled)
                {
                    if (rp.rect.TryFindRandomInnerRect(new IntVec2(9, 9), out CellRect rect, null))
                    {
                        ResolveParams resolveParams = rp;
                        resolveParams.rect = rect;
                        BaseGen.symbolStack.Push("landingPad", resolveParams, null);
                        BaseGen.globalSettings.basePart_landingPadsResolved++;
                    }
                }

                // Push ruin symbol
                BaseGen.symbolStack.Push("kcsg_handleruins", rp, null);

                // Start gen
                SettlementGenUtils.StartGen(rp, map, GenOption.sld);
            }
        }

        private void AddHostilePawnGroup(Faction faction, Map map, ResolveParams parms)
        {
            Lord singlePawnLord;
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.options.Any(k => !k.kind.RaceProps.EatsFood)))
                singlePawnLord = parms.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBaseNoEat(faction, parms.rect.CenterCell), map, null);
            else
                singlePawnLord = parms.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, parms.rect.CenterCell), map, null);

            TraverseParms tp = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams rp = parms;
            rp.rect = parms.rect;
            rp.faction = faction;
            rp.singlePawnLord = singlePawnLord;
            rp.pawnGroupKindDef = GenOption.sld?.groupKindDef ?? parms.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
            rp.singlePawnSpawnCellExtraPredicate = parms.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, tp));
            if (rp.pawnGroupMakerParams == null && faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
            {
                rp.pawnGroupMakerParams = new PawnGroupMakerParms
                {
                    tile = map.Tile,
                    faction = faction,
                    points = parms.settlementPawnGroupPoints ?? RimWorld.BaseGen.SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange,
                    inhabitants = true,
                    seed = parms.settlementPawnGroupSeed
                };
            }
            if (GenOption.sld != null) rp.pawnGroupMakerParams.points *= GenOption.sld.pawnGroupMultiplier;

            BaseGen.symbolStack.Push("pawnGroup", rp, null);
        }
    }
}