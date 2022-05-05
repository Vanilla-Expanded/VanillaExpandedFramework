using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    public class GenUtils
    {
        public static void GenerateRoomFromLayout(StructureLayoutDef layout, int index, CellRect rect, Map map)
        {
            Faction faction = map.ParentFaction;

            var cells = rect.Cells.ToList();
            int count = cells.Count;

            for (int i = 0; i < count; i++)
            {
                IntVec3 cell = cells[i];
                if (cell.InBounds(map))
                {
                    SymbolDef temp = layout.symbolsLists[index][i];
                    if (temp != null)
                    {
                        if (temp.isTerrain && temp.terrainDef != null)
                        {
                            GenerateTerrainAt(map, cell, temp.terrainDef);
                        }
                        else if (temp.pawnKindDefNS != null && CGO.factionSettlement?.shouldRuin == false)
                        {
                            GeneratePawnAt(map, cell, temp);
                        }
                        else if (temp.thingDef != null)
                        {
                            if (temp.thingDef.category == ThingCategory.Item)
                            {
                                GenerateItemAt(map, cell, temp);
                            }
                            else if (temp.thingDef.category == ThingCategory.Plant)
                            {
                                Plant plant = ThingMaker.MakeThing(temp.thingDef) as Plant;
                                plant.Growth = temp.plantGrowth;
                                GenSpawn.Spawn(plant, cell, map, WipeMode.VanishOrMoveAside);
                            }
                            else if (temp.thingDef.category == ThingCategory.Pawn)
                            {
                                if (CGO.factionSettlement?.shouldRuin == true)
                                {
                                    continue;
                                }
                                GenSpawn.Spawn(temp.thingDef, cell, map, WipeMode.VanishOrMoveAside);
                            }
                            else
                            {
                                if (cell.GetFirstMineable(map) != null && temp.thingDef.designationCategory == DesignationCategoryDefOf.Security)
                                {
                                    continue;
                                }
                                GenerateBuildingAt(map, cell, temp, faction, layout.spawnConduits);
                            }
                        }
                    }
                }
            }
        }

        // Gen methods
        public static void GenerateTerrainAt(Map map, IntVec3 cell, TerrainDef terrainDef)
        {
            if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
            {
                map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
            }
            else
            {
                cell.GetFirstMineable(map)?.DeSpawn();
                map.terrainGrid.SetTerrain(cell, terrainDef);
            }
        }

        public static void GeneratePawnAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            bool parentFaction = map.ParentFaction != null;

            if (symbol.lordJob != null)
            {
                Lord lord = CreateNewLord(symbol.lordJob, map, cell);
                for (int i = 0; i < symbol.numberToSpawn; i++)
                {
                    Pawn pawn = symbol.spawnPartOfFaction ? PawnGenerator.GeneratePawn(symbol.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(symbol.pawnKindDefNS);
                    if (pawn != null)
                    {
                        if (symbol.isSlave && parentFaction)
                        {
                            pawn.guest.SetGuestStatus(map.ParentFaction, GuestStatus.Prisoner);
                        }

                        GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                        lord.AddPawn(pawn);
                    }
                }
            }
            else
            {
                for (int i = 0; i < symbol.numberToSpawn; i++)
                {
                    Pawn pawn = symbol.spawnPartOfFaction ? PawnGenerator.GeneratePawn(symbol.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(symbol.pawnKindDefNS);
                    if (pawn != null)
                    {
                        if (symbol.isSlave && parentFaction)
                        {
                            pawn.guest.SetGuestStatus(map.ParentFaction, GuestStatus.Prisoner);
                        }

                        GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                    }
                }
            }
        }

        public static void GenerateItemAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            Thing thing = ThingMaker.MakeThing(symbol.thingDef, symbol.stuffDef ?? (symbol.thingDef.stuffCategories?.Count > 0 ? GenStuff.RandomStuffFor(symbol.thingDef) : null));

            if (symbol.maxStackSize != -1)
            {
                thing.stackCount = Rand.RangeInclusive(1, symbol.maxStackSize);
            }
            else
            {
                thing.stackCount = Mathf.Clamp(Rand.RangeInclusive(1, symbol.thingDef.stackLimit), 1, 75);
            }

            thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);

            GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Direct);
            thing.SetForbidden(true, false);
        }

        public static void GenerateBuildingAt(Map map, IntVec3 cell, SymbolDef symbol, Faction faction, bool generateConduit)
        {
            Thing thing = ThingMaker.MakeThing(symbol.thingDef, symbol.thingDef.CostStuffCount > 0 ? (symbol.stuffDef ?? symbol.thingDef.defaultStuff ?? ThingDefOf.WoodLog) : null);

            CompRefuelable refuelable = thing.TryGetComp<CompRefuelable>();
            refuelable?.Refuel(refuelable.Props.fuelCapacity);

            CompPowerBattery battery = thing.TryGetComp<CompPowerBattery>();
            battery?.AddEnergy(battery.Props.storedEnergyMax);

            if (thing is Building_CryptosleepCasket cryptosleepCasket && Rand.Value < symbol.chanceToContainPawn)
            {
                Pawn pawn = GeneratePawnForContainer(symbol, map);
                if (!cryptosleepCasket.TryAcceptThing(pawn))
                {
                    pawn.Destroy();
                }
            }
            else if (thing is Building_CorpseCasket corpseCasket && Rand.Value < symbol.chanceToContainPawn)
            {
                Pawn pawn = GeneratePawnForContainer(symbol, map);
                if (!corpseCasket.TryAcceptThing(pawn))
                {
                    pawn.Destroy();
                }
            }
            else if (thing is Building_Crate crate)
            {
                List<Thing> thingList = new List<Thing>();
                if (faction == Faction.OfPlayer && symbol.thingSetMakerDefForPlayer != null)
                {
                    thingList = symbol.thingSetMakerDefForPlayer.root.Generate(new ThingSetMakerParams());
                }
                else if (symbol.thingSetMakerDef != null)
                {
                    thingList = symbol.thingSetMakerDef.root.Generate(new ThingSetMakerParams());
                }

                foreach (Thing t in thingList)
                {
                    t.stackCount = Math.Min((int)(t.stackCount * symbol.crateStackMultiplier), t.def.stackLimit);
                }

                thingList.ForEach(t =>
                {
                    if (!crate.TryAcceptThing(t, false))
                    {
                        t.Destroy();
                    }
                });
            }

            if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
            {
                if (thing.def.building.isNaturalRock)
                {
                    TerrainDef t = DefDatabase<TerrainDef>.GetNamedSilentFail($"{thing.def.defName}_Rough");
                    if (t != null)
                    {
                        map.terrainGrid.SetTerrain(cell, t);
                        foreach (IntVec3 intVec3 in CellRect.CenteredOn(cell, 1))
                        {
                            if (!intVec3.GetTerrain(map).BuildableByPlayer)
                            {
                                map.terrainGrid.SetTerrain(intVec3, t);
                            }
                        }
                    }
                }
                else
                {
                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
                }
            }

            GenSpawn.Spawn(thing, cell, map, symbol.rotation, WipeMode.VanishOrMoveAside);

            if (faction != null && thing.def.CanHaveFaction)
            {
                thing.SetFactionDirect(faction);
            }

            if (generateConduit && !thing.def.mineable && (thing.def.passability == Traversability.Impassable || thing.def.IsDoor) && faction?.def.techLevel >= TechLevel.Industrial) // Add power cable under all impassable
            {
                Thing c = ThingMaker.MakeThing(ThingDefOf.PowerConduit);
                if (faction != null)
                {
                    c.SetFactionDirect(faction);
                }

                GenSpawn.Spawn(c, cell, map, WipeMode.FullRefund);
            }
            // Handle mortar and mortar pawns
            SpawnMortar(thing, faction, map);
        }

        private static void SpawnMortar(Thing thing, Faction faction, Map map)
        {
            if (thing?.def?.building?.buildingTags?.Count > 0)
            {
                if (thing.def.building.IsMortar && thing.def.category == ThingCategory.Building && thing.def.building.buildingTags.Contains("Artillery_MannedMortar") && thing.def.HasComp(typeof(CompMannable)) && faction != null)
                {
                    // Spawn pawn
                    Lord singlePawnLord = LordMaker.MakeNewLord(faction, new LordJob_ManTurrets(), map, null);
                    PawnGenerationRequest value = new PawnGenerationRequest(faction.RandomPawnKind(), faction, PawnGenerationContext.NonPlayer, map.Tile, mustBeCapableOfViolence: true, inhabitant: true);
                    ResolveParams rpPawn = new ResolveParams
                    {
                        faction = faction,
                        singlePawnGenerationRequest = new PawnGenerationRequest?(value),
                        rect = CellRect.SingleCell(thing.InteractionCell),
                        singlePawnLord = singlePawnLord
                    };
                    BaseGen.symbolStack.Push("pawn", rpPawn);
                    // Spawn shells
                    ThingDef shellDef = TurretGunUtility.TryFindRandomShellDef(thing.def, false, true, faction.def.techLevel, false, 250f);
                    if (shellDef != null)
                    {
                        ResolveParams rpShell = new ResolveParams
                        {
                            faction = faction,
                            singleThingDef = shellDef,
                            singleThingStackCount = Rand.RangeInclusive(8, Math.Min(12, shellDef.stackLimit))
                        };
                        BaseGen.symbolStack.Push("thing", rpShell);
                    }
                }
            }
        }


        private static Pawn GeneratePawnForContainer(SymbolDef temp, Map map)
        {
            Faction faction = temp.spawnPartOfFaction ? map.ParentFaction : null;
            if (temp.containPawnKindForPlayerAnyOf.Count > 0 && faction == Faction.OfPlayer)
            {
                return PawnGenerator.GeneratePawn(new PawnGenerationRequest(temp.containPawnKindForPlayerAnyOf.RandomElement(), faction, forceGenerateNewPawn: true, certainlyBeenInCryptosleep: true));
            }
            else if (temp.containPawnKindAnyOf.Count > 0)
            {
                return PawnGenerator.GeneratePawn(new PawnGenerationRequest(temp.containPawnKindAnyOf.RandomElement(), faction, forceGenerateNewPawn: true, certainlyBeenInCryptosleep: true));
            }

            return PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, faction);
        }

        public static void GenerateRoofGrid(StructureLayoutDef layout, CellRect rect, Map map)
        {
            var cells = rect.Cells.ToList();
            int count = cells.Count;

            for (int i = 0; i < count; i++)
            {
                IntVec3 cell = cells[i];
                if (cell.InBounds(map))
                {
                    switch (layout.roofGridResolved[i])
                    {
                        case "1":
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
                            break;
                        case "2":
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThin);
                            break;
                        case "3":
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                            break;
                        default:
                            break;
                    }
                }

            }
        }

        public static Lord CreateNewLord(Type lordJobType, Map map, IntVec3 cell)
        {
            return LordMaker.MakeNewLord(map.ParentFaction, Activator.CreateInstance(lordJobType, new object[]
            {
                new SpawnedPawnParams
                {
                    defSpot = cell,
                }
            }) as LordJob, map, null);
        }

        public static IntVec3 FindRect(Map map, int h, int w, bool nearCenter = false)
        {
            CellRect rect;
            int fromCenter = 1;
            while (true)
            {
                if (nearCenter)
                {
                    rect = CellRect.CenteredOn(CellFinder.RandomClosewalkCellNear(map.Center, map, fromCenter), w, h);
                    rect.ClipInsideMap(map);
                }
                else
                {
                    rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(h, map), w, h);
                }

                if (rect.Cells.ToList().Any(i => !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium)))
                {
                    fromCenter += 2;
                }
                else
                {
                    return rect.CenterCell;
                }
            }
        }

        public static void PreClean(Map map, CellRect rect, List<string> roofGrid, bool fullClean)
        {
            KLog.Message($"Intiating pre-generation map clean - only under roof. Fullclean {fullClean}");
            SetRoadInfo(map);
            List<string> rg = new List<string>();
            foreach (string str in roofGrid)
            {
                rg.AddRange(str.Split(','));
            }

            for (int i = 0; i < rg.Count; i++)
            {
                if (rg[i] != null && rg[i] != ".")
                {
                    IntVec3 c = rect.Cells.ElementAt(i);
                    CleanAt(c, map, fullClean);
                    CleanTerrainAt(c, map);
                }
            }
        }

        public static void PreClean(Map map, CellRect rect, bool fullClean)
        {
            KLog.Message($"Intiating pre-generation map clean - full rect. Fullclean {fullClean}");
            SetRoadInfo(map);
            foreach (IntVec3 c in rect)
            {
                CleanAt(c, map, fullClean);
                CleanTerrainAt(c, map);
            }
        }

        public static void SetRoadInfo(Map map)
        {
            if (map.TileInfo?.Roads?.Count > 0)
            {
                CGO.preRoadTypes = new List<TerrainDef>();
                foreach (RimWorld.Planet.Tile.RoadLink roadLink in map.TileInfo.Roads)
                {
                    foreach (RoadDefGenStep rgs in roadLink.road.roadGenSteps)
                    {
                        if (rgs is RoadDefGenStep_Place rgsp && rgsp != null && rgsp.place is TerrainDef t && t != null && t != TerrainDefOf.Bridge)
                        {
                            CGO.preRoadTypes.Add(t);
                        }
                    }
                }
            }
        }

        public static void CleanAt(IntVec3 c, Map map, bool fullClean)
        {
            if (fullClean)
            {
                c.GetThingList(map).ToList().ForEach((t) =>
                {
                    if (t.Map != null)
                    {
                        if (t is Pawn p && p.Faction == map.ParentFaction)
                        {
                            KLog.Message($"Skipping {p.NameShortColored}, part of defender faction.");
                        }
                        else
                        {
                            t.DeSpawn();
                        }
                    }
                });
            }
            else
            {
                c.GetThingList(map).ToList().ForEach((t) =>
                {
                    if (t.Map != null &&
                       (t.def.category == ThingCategory.Filth ||
                        t.def.category == ThingCategory.Building && !t.def.building.isNaturalRock ||
                        t is Pawn p && p.Faction != map.ParentFaction ||
                        t.def.thingCategories != null && t.def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks)))
                    {
                        t.DeSpawn();
                    }
                });
            }
        }

        public static void CleanTerrainAt(IntVec3 c, Map map)
        {
            if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
            {
                map.terrainGrid.SetTerrain(c, terrain);
            }
        }

        #region Power Function

        // Vanilla function, remade to be able to use subsurface conduit when mod loaded
        public static void EnsureBatteriesConnectedAndMakeSense(Map map, List<Thing> tmpThings, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults, List<IntVec3> tmpCells, ThingDef conduit, bool spawnTransmitters = true)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                CompPowerBattery compPowerBattery = tmpThings[i].TryGetComp<CompPowerBattery>();
                if (compPowerBattery != null)
                {
                    PowerNet powerNet = compPowerBattery.PowerNet;
                    if (powerNet == null)
                    {
                        map.powerNetManager.UpdatePowerNetsAndConnections_First();
                        if (TryFindClosestReachableNet(compPowerBattery.parent.Position, (PowerNet x) => HasAnyPowerGenerator(x), map, out PowerNet powerNet2, out IntVec3 dest, tmpPowerNetPredicateResults))
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            if (powerNet2 != null && spawnTransmitters)
                            {
                                SpawnTransmitters(tmpCells, map, compPowerBattery.parent.Faction, conduit);
                            }
                        }
                    }
                }
            }
        }

        public static void EnsurePowerUsersConnected(Map map, List<Thing> tmpThings, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults, List<IntVec3> tmpCells, ThingDef conduit, bool spawnTransmitters = true)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            bool hasAtleast1TurretInt = tmpThings.Any((Thing t) => t is Building_Turret);
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (IsPowerUser(tmpThings[i]))
                {
                    CompPowerTrader powerComp = tmpThings[i].TryGetComp<CompPowerTrader>();
                    PowerNet powerNet = powerComp.PowerNet;
                    if (powerNet != null && powerNet.hasPowerSource)
                    {
                        TryTurnOnImmediately(powerComp, map);
                    }
                    else
                    {
                        map.powerNetManager.UpdatePowerNetsAndConnections_First();
                        if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentEnergyGainRate() - powerComp.Props.basePowerConsumption * CompPower.WattsToWattDaysPerTick > 1E-07f, map, out PowerNet powerNet2, out IntVec3 dest, tmpPowerNetPredicateResults) && spawnTransmitters)
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
                            TryTurnOnImmediately(powerComp, map);
                        }
                        else if (TryFindClosestReachableNet(powerComp.parent.Position, (PowerNet x) => x.CurrentStoredEnergy() > 1E-07f, map, out powerNet2, out dest, tmpPowerNetPredicateResults) && spawnTransmitters)
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
                        }
                    }
                }
            }
        }

        public static void EnsureGeneratorsConnectedAndMakeSense(Map map, List<Thing> tmpThings, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults, List<IntVec3> tmpCells, ThingDef conduit, bool spawnTransmitters = true)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (IsPowerGenerator(tmpThings[i]))
                {
                    PowerNet powerNet = tmpThings[i].TryGetComp<CompPower>().PowerNet;
                    if (powerNet == null || !HasAnyPowerUser(powerNet))
                    {
                        map.powerNetManager.UpdatePowerNetsAndConnections_First();
                        if (TryFindClosestReachableNet(tmpThings[i].Position, (PowerNet x) => HasAnyPowerUser(x), map, out PowerNet powerNet2, out IntVec3 dest, tmpPowerNetPredicateResults) && spawnTransmitters)
                        {
                            map.floodFiller.ReconstructLastFloodFillPath(dest, tmpCells);
                            SpawnTransmitters(tmpCells, map, tmpThings[i].Faction, conduit);
                        }
                    }
                }
            }
        }

        public static bool HasAnyPowerUser(PowerNet net)
        {
            List<CompPowerTrader> powerComps = net.powerComps;
            for (int i = 0; i < powerComps.Count; i++)
            {
                if (IsPowerUser(powerComps[i].parent))
                {
                    return true;
                }
            }
            return false;
        }

        public static void TryTurnOnImmediately(CompPowerTrader powerComp, Map map)
        {
            if (powerComp.PowerOn)
            {
                return;
            }
            map.powerNetManager.UpdatePowerNetsAndConnections_First();
            if (powerComp.PowerNet != null && powerComp.PowerNet.CurrentEnergyGainRate() > 1E-07f)
            {
                powerComp.PowerOn = true;
            }
        }

        public static bool IsPowerUser(Thing thing)
        {
            CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
            return compPowerTrader != null && (compPowerTrader.PowerOutput < 0f || (!compPowerTrader.PowerOn && compPowerTrader.Props.basePowerConsumption > 0f));
        }

        public static void SpawnTransmitters(List<IntVec3> cells, Map map, Faction faction, ThingDef conduit)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].GetTransmitter(map) == null)
                {
                    GenSpawn.Spawn(conduit, cells[i], map, WipeMode.FullRefund).SetFaction(faction, null);
                }
            }
        }

        public static bool EverPossibleToTransmitPowerAt(IntVec3 c, Map map)
        {
            return c.GetTransmitter(map) != null || GenConstruct.CanBuildOnTerrain(ThingDefOf.PowerConduit, c, map, Rot4.North, null, null);
        }

        public static bool IsPowerGenerator(Thing thing)
        {
            if (thing.TryGetComp<CompPowerPlant>() != null)
            {
                return true;
            }
            CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
            return compPowerTrader != null && (compPowerTrader.PowerOutput > 0f || (!compPowerTrader.PowerOn && compPowerTrader.Props.basePowerConsumption < 0f));
        }

        public static bool HasAnyPowerGenerator(PowerNet net)
        {
            List<CompPowerTrader> powerComps = net.powerComps;
            for (int i = 0; i < powerComps.Count; i++)
            {
                if (IsPowerGenerator(powerComps[i].parent))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool TryFindClosestReachableNet(IntVec3 root, Predicate<PowerNet> predicate, Map map, out PowerNet foundNet, out IntVec3 closestTransmitter, Dictionary<PowerNet, bool> tmpPowerNetPredicateResults)
        {
            tmpPowerNetPredicateResults.Clear();
            PowerNet foundNetLocal = null;
            IntVec3 closestTransmitterLocal = IntVec3.Invalid;
            map.floodFiller.FloodFill(root, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate (IntVec3 x)
            {
                Building transmitter = x.GetTransmitter(map);
                PowerNet powerNet = transmitter?.GetComp<CompPower>().PowerNet;
                if (powerNet == null)
                {
                    return false;
                }
                if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out bool flag))
                {
                    flag = predicate(powerNet);
                    tmpPowerNetPredicateResults.Add(powerNet, flag);
                }
                if (flag)
                {
                    foundNetLocal = powerNet;
                    closestTransmitterLocal = x;
                    return true;
                }
                return false;
            }, int.MaxValue, true, null);
            tmpPowerNetPredicateResults.Clear();
            if (foundNetLocal != null)
            {
                foundNet = foundNetLocal;
                closestTransmitter = closestTransmitterLocal;
                return true;
            }
            foundNet = null;
            closestTransmitter = IntVec3.Invalid;
            return false;
        }

        #endregion Power Function
    }
}