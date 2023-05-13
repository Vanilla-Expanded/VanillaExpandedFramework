using System;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class SymbolResolver_EdgeDefenseCustomizable : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            if (faction == null)
                return;

            Map map = BaseGen.globalSettings.map;
            var defenseOptions = GenOption.settlementLayout.defenseOptions;

            int width = rp.edgeDefenseWidth ?? (!rp.edgeDefenseMortarsCount.HasValue || rp.edgeDefenseMortarsCount.Value <= 0 ? (Rand.Bool ? 2 : 4) : 4);
            width = Mathf.Clamp(width, 1, Mathf.Min(rp.rect.Width, rp.rect.Height) / 2);

            int turretCount = 0, mortarCount = 0;
            bool turretIS = true, mortarIS = true, sandbagIS = true;

            if (faction.def.techLevel >= TechLevel.Industrial)
            {
                switch (width)
                {
                    case 1:
                        turretCount = rp.edgeDefenseTurretsCount ?? 0;
                        mortarCount = 0;
                        sandbagIS = false;
                        break;

                    case 2:
                        turretCount = rp.edgeDefenseTurretsCount ?? rp.rect.EdgeCellsCount / defenseOptions.cellsPerTurret;
                        mortarCount = 0;
                        turretIS = false;
                        sandbagIS = false;
                        break;

                    case 3:
                        turretCount = rp.edgeDefenseTurretsCount ?? rp.rect.EdgeCellsCount / defenseOptions.cellsPerTurret;
                        mortarCount = rp.edgeDefenseMortarsCount ?? rp.rect.EdgeCellsCount / defenseOptions.cellsPerMortar;
                        turretIS = false;
                        sandbagIS = mortarCount == 0;
                        break;

                    default:
                        turretCount = rp.edgeDefenseTurretsCount ?? rp.rect.EdgeCellsCount / defenseOptions.cellsPerTurret;
                        mortarCount = rp.edgeDefenseMortarsCount ?? rp.rect.EdgeCellsCount / defenseOptions.cellsPerMortar;
                        turretIS = false;
                        mortarIS = false;
                        break;
                }
            }

            if (defenseOptions.addSandbags)
            {
                CellRect sandbagsRect = rp.rect;
                for (int index = 0; index < width; ++index)
                {
                    if (index % 2 == 0)
                    {
                        ResolveParams rpSandbags = rp;
                        rpSandbags.faction = faction;
                        rpSandbags.rect = sandbagsRect;
                        BaseGen.symbolStack.Push("edgeSandbags", rpSandbags);
                        if (!sandbagIS)
                            break;
                    }
                    sandbagsRect = sandbagsRect.ContractedBy(1);
                }
            }

            if (defenseOptions.addMortars)
            {
                CellRect mortarRect = mortarIS ? rp.rect : rp.rect.ContractedBy(1);
                for (int index = 0; index < mortarCount; ++index)
                {
                    // Spawn mortar
                    Rot4 rot = rp.thingRot ?? Rot4.Random;
                    ThingDef thingDef = defenseOptions.allowedMortarsDefs.RandomElement();
                    if (!TryFindMortarSpawnCell(rp.rect, rot, thingDef, out IntVec3 cell))
                        return;

                    var thing = ThingMaker.MakeThing(thingDef, thingDef.MadeFromStuff ? GenStuff.RandomStuffFor(thingDef) : null);
                    GenSpawn.Spawn(thing, cell, map, rot);

                    // Spawn pawn
                    var request = new PawnGenerationRequest(faction.RandomPawnKind(), faction, PawnGenerationContext.NonPlayer, map.Tile, mustBeCapableOfViolence: true, inhabitant: true);
                    var pawn = PawnGenerator.GeneratePawn(request);
                    var job = JobMaker.MakeJob(JobDefOf.ManTurret, thing);
                    job.expiryInterval = 20000;
                    job.expireRequiresEnemiesNearby = true;

                    GenSpawn.Spawn(pawn, thing.InteractionCell, map);
                    pawn.jobs.TryTakeOrderedJob(job);
                    // Spawn shells
                    ThingDef shellDef = TurretGunUtility.TryFindRandomShellDef(thing.def, false, true, true, faction.def.techLevel, false, 250f);
                    if (shellDef != null)
                    {
                        ResolveParams rpShell = new ResolveParams
                        {
                            faction = faction,
                            singleThingDef = shellDef,
                            singleThingStackCount = Rand.RangeInclusive(8, Math.Min(15, shellDef.stackLimit))
                        };
                        BaseGen.symbolStack.Push("thing", rpShell);
                    }
                }
            }

            if (defenseOptions.addTurrets)
            {
                CellRect turretRect = turretIS ? rp.rect : rp.rect.ContractedBy(1);
                for (int index = 0; index < turretCount; ++index)
                {
                    ResolveParams rpTurret = rp;
                    rpTurret.faction = faction;
                    rpTurret.singleThingDef = defenseOptions.allowedTurretsDefs.RandomElement();
                    rpTurret.rect = turretRect;
                    rpTurret.edgeThingAvoidOtherEdgeThings = true;

                    BaseGen.symbolStack.Push("edgeThing", rpTurret);
                }
            }
        }

        private bool TryFindMortarSpawnCell(CellRect rect, Rot4 rot, ThingDef mortarDef, out IntVec3 cell)
        {
            Map map = BaseGen.globalSettings.map;
            Predicate<CellRect> edgeTouchCheck = !(rot == Rot4.North) ? (!(rot == Rot4.South) ? (!(rot == Rot4.West) ? (x => x.Cells.Any(y => y.x == rect.maxX)) : (Predicate<CellRect>)(x => x.Cells.Any(y => y.x == rect.minX))) : (x => x.Cells.Any(y => y.z == rect.minZ))) : (x => x.Cells.Any(y => y.z == rect.maxZ));
            return CellFinder.TryFindRandomCellInsideWith(rect, x =>
            {
                CellRect cellRect = GenAdj.OccupiedRect(x, rot, mortarDef.size);
                return ThingUtility.InteractionCellWhenAt(mortarDef, x, rot, map).Standable(map) && cellRect.FullyContainedWithin(rect) && edgeTouchCheck(cellRect);
            }, out cell);
        }
    }
}