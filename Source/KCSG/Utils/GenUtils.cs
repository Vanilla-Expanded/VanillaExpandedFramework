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
        public static void GenerateRoomFromLayout(List<string> layoutList, CellRect roomRect, Map map, StructureLayoutDef rld, bool generateConduit = true)
        {
            bool parentFaction = map.ParentFaction != null;

            if (rld.roofGrid != null)
            {
                GenerateRoofGrid(rld.roofGrid, roomRect, map);
            }

            List<string> allSymbList = new List<string>();
            foreach (string str in layoutList)
            {
                allSymbList.AddRange(str.Split(','));
            }

            int l = 0;
            foreach (IntVec3 cell in roomRect)
            {
                if (l < allSymbList.Count && allSymbList[l] != ".")
                {
                    SymbolDef temp = DefDatabase<SymbolDef>.GetNamedSilentFail(allSymbList[l]);
                    Thing thing;
                    if (temp != null)
                    {
                        if (temp.isTerrain && temp.terrainDef != null)
                        {
                            GenerateTerrainAt(map, cell, temp.terrainDef);
                        }
                        else if (temp.pawnKindDefNS != null && CGO.factionSettlement?.shouldRuin == false)
                        {
                            if (temp.lordJob != null)
                            {
                                Lord lord = CreateNewLord(temp.lordJob, map, cell);
                                for (int i = 0; i < temp.numberToSpawn; i++)
                                {
                                    Pawn pawn = temp.spawnPartOfFaction ? PawnGenerator.GeneratePawn(temp.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(temp.pawnKindDefNS);
                                    if (pawn != null)
                                    {
                                        if (temp.isSlave && parentFaction) pawn.guest.SetGuestStatus(map.ParentFaction, GuestStatus.Prisoner);

                                        GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                                        lord.AddPawn(pawn);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < temp.numberToSpawn; i++)
                                {
                                    Pawn pawn = temp.spawnPartOfFaction ? PawnGenerator.GeneratePawn(temp.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(temp.pawnKindDefNS);
                                    if (pawn != null)
                                    {
                                        if (temp.isSlave && parentFaction) pawn.guest.SetGuestStatus(map.ParentFaction, GuestStatus.Prisoner);
                                        GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                                    }
                                }
                            }
                        }
                        else if (temp.thingDef?.category == ThingCategory.Item && cell.Walkable(map))
                        {
                            thing = ThingMaker.MakeThing(temp.thingDef, temp.thingDef.stuffCategories?.Count > 0 ? GenStuff.RandomStuffFor(temp.thingDef) : null);
                            thing.stackCount = Mathf.Clamp(Rand.RangeInclusive(1, temp.thingDef.stackLimit), 1, 75);

                            CompQuality quality = thing.TryGetComp<CompQuality>();
                            quality?.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);

                            GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
                            thing.SetForbidden(true, false);
                        }
                        else if (temp.thingDef != null)
                        {
                            thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);

                            CompRefuelable refuelable = thing.TryGetComp<CompRefuelable>();
                            refuelable?.Refuel(refuelable.Props.fuelCapacity);

                            CompPowerBattery battery = thing.TryGetComp<CompPowerBattery>();
                            battery?.AddEnergy(battery.Props.storedEnergyMax);

                            if (thing is Building_CryptosleepCasket cryptosleepCasket && Rand.Value < temp.chanceToContainPawn)
                            {
                                Pawn pawn = GeneratePawnForContainer(temp, map);
                                if (!cryptosleepCasket.TryAcceptThing(pawn))
                                    pawn.Destroy();
                            }
                            else if (thing is Building_CorpseCasket corpseCasket && Rand.Value < temp.chanceToContainPawn)
                            {
                                Pawn pawn = GeneratePawnForContainer(temp, map);
                                if (!corpseCasket.TryAcceptThing(pawn))
                                    pawn.Destroy();
                            }
                            else if (thing is Building_Crate crate)
                            {
                                List<Thing> thingList = new List<Thing>();
                                if (map.ParentFaction == Faction.OfPlayer && temp.thingSetMakerDefForPlayer != null)
                                {
                                    thingList = temp.thingSetMakerDefForPlayer.root.Generate(new ThingSetMakerParams());
                                }
                                else if (temp.thingSetMakerDef != null)
                                {
                                    thingList = temp.thingSetMakerDef.root.Generate(new ThingSetMakerParams());
                                }

                                foreach (Thing t in thingList)
                                {
                                    t.stackCount = Math.Min((int)(t.stackCount * temp.crateStackMultiplier), t.def.stackLimit);
                                }

                                thingList.ForEach(t =>
                                {
                                    if (!crate.TryAcceptThing(t, false))
                                        t.Destroy();
                                });
                            }

                            if (thing.def.category == ThingCategory.Pawn && CGO.factionSettlement?.shouldRuin == true)
                            {
                                l++;
                                continue;
                            }
                            else if (cell.GetFirstMineable(map) is Mineable m && thing.def.designationCategory == DesignationCategoryDefOf.Security)
                            {
                                l++;
                                continue;
                            }
                            else if (thing.def.category == ThingCategory.Plant && cell.GetTerrain(map).fertility > 0.5 && cell.Walkable(map)) // If it's a plant
                            {
                                Plant plant = thing as Plant;
                                plant.Growth = temp.plantGrowth; // apply the growth
                                GenSpawn.Spawn(plant, cell, map, WipeMode.VanishOrMoveAside);
                            }
                            else if (thing.def.category == ThingCategory.Building)
                            {
                                if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy) && !thing.def.building.isNaturalRock)
                                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);

                                if (thing.def.rotatable)
                                    GenSpawn.Spawn(thing, cell, map, new Rot4(temp.rotation.AsInt), WipeMode.VanishOrMoveAside);
                                else
                                    GenSpawn.Spawn(thing, cell, map, WipeMode.VanishOrMoveAside);

                                if (parentFaction) thing.SetFactionDirect(map.ParentFaction);
                            }

                            if (generateConduit && rld.spawnConduits && !thing.def.mineable && (thing.def.passability == Traversability.Impassable || thing.def.IsDoor) && map.ParentFaction?.def.techLevel >= TechLevel.Industrial) // Add power cable under all impassable
                            {
                                Thing c = ThingMaker.MakeThing(ThingDefOf.PowerConduit);
                                if (parentFaction) c.SetFactionDirect(map.ParentFaction);
                                GenSpawn.Spawn(c, cell, map, WipeMode.FullRefund);
                            }
                            // Handle mortar and mortar pawns
                            if (thing?.def?.building?.buildingTags?.Count > 0)
                            {
                                if (thing.def.building.IsMortar && thing.def.category == ThingCategory.Building && thing.def.building.buildingTags.Contains("Artillery_MannedMortar") && thing.def.HasComp(typeof(CompMannable)) && parentFaction)
                                {
                                    // Spawn pawn
                                    Lord singlePawnLord = LordMaker.MakeNewLord(map.ParentFaction, new LordJob_ManTurrets(), map, null);
                                    PawnGenerationRequest value = new PawnGenerationRequest(map.ParentFaction.RandomPawnKind(), map.ParentFaction, PawnGenerationContext.NonPlayer, map.Tile, mustBeCapableOfViolence: true, inhabitant: true);
                                    ResolveParams rpPawn = new ResolveParams
                                    {
                                        faction = map.ParentFaction,
                                        singlePawnGenerationRequest = new PawnGenerationRequest?(value),
                                        rect = CellRect.SingleCell(thing.InteractionCell),
                                        singlePawnLord = singlePawnLord
                                    };
                                    BaseGen.symbolStack.Push("pawn", rpPawn);
                                    // Spawn shells
                                    ThingDef shellDef = TurretGunUtility.TryFindRandomShellDef(thing.def, false, true, map.ParentFaction.def.techLevel, false, 250f);
                                    if (shellDef != null)
                                    {
                                        ResolveParams rpShell = new ResolveParams
                                        {
                                            faction = map.ParentFaction,
                                            singleThingDef = shellDef,
                                            singleThingStackCount = Rand.RangeInclusive(8, Math.Min(12, shellDef.stackLimit))
                                        };
                                        BaseGen.symbolStack.Push("thing", rpShell);
                                    }
                                }
                            }
                        }
                        else if (VFECore.VFEGlobal.settings.enableVerboseLogging)
                        {
                            Log.Message($"SymbolDef for {allSymbList[l]} wasn't able to be used.");
                        }
                    }
                    else if (VFECore.VFEGlobal.settings.enableVerboseLogging)
                    {
                        Log.Message($"Null symbolDef for {allSymbList[l]}, ignoring.");
                    }
                }
                l++;
            }
        }

        private static Pawn GeneratePawnForContainer(SymbolDef temp, Map map)
        {
            // Kept for compat reasons
            // TODO Remove
            Faction faction = temp.spawnPartOfFaction ? (temp.faction != null ? Find.FactionManager.FirstFactionOfDef(temp.faction) : map.ParentFaction) : null;
            if (temp.containPawnKindDefForPlayer != null && map.ParentFaction == Faction.OfPlayer)
            {
                return PawnGenerator.GeneratePawn(temp.containPawnKindDefForPlayer, faction);
            }
            else if (temp.containPawnKindDef != null)
            {
                return PawnGenerator.GeneratePawn(temp.containPawnKindDef, faction);
            }
            else if (temp.containPawnKindForPlayerAnyOf.Count > 0 && map.ParentFaction == Faction.OfPlayer)
            {
                return PawnGenerator.GeneratePawn(temp.containPawnKindForPlayerAnyOf.RandomElement(), faction);
            } 
            else if (temp.containPawnKindAnyOf.Count > 0)
            {
                return PawnGenerator.GeneratePawn(temp.containPawnKindAnyOf.RandomElement(), faction);
            }            

            return PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, faction);
        }

        public static void GenerateRoofGrid(List<string> roofGrid, CellRect roomRect, Map map)
        {
            List<string> rg = new List<string>();
            foreach (string str in roofGrid)
            {
                rg.AddRange(str.Split(','));
            }

            for (int i = 0; i < rg.Count; i++)
            {
                switch (rg[i])
                {
                    case "1":
                        map.roofGrid.SetRoof(roomRect.Cells.ElementAt(i), RoofDefOf.RoofConstructed);
                        break;
                    case "2":
                        map.roofGrid.SetRoof(roomRect.Cells.ElementAt(i), RoofDefOf.RoofRockThin);
                        break;
                    case "3":
                        map.roofGrid.SetRoof(roomRect.Cells.ElementAt(i), RoofDefOf.RoofRockThick);
                        break;
                    default:
                        break;
                }

                if (rg[i] != "." && !roomRect.Cells.ElementAt(i).GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                {
                    map.terrainGrid.SetTerrain(roomRect.Cells.ElementAt(i), TerrainDefOf.Bridge);
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
                else rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(h, map), w, h);

                if (rect.Cells.ToList().Any(i => !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium))) fromCenter += 2;
                else return rect.CenterCell;
            }
        }

        public static void PreClean(Map map, CellRect rect, List<string> roofGrid, bool fullClean)
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

            List<string> rg = new List<string>();
            foreach (string str in roofGrid)
            {
                rg.AddRange(str.Split(','));
            }

            for (int i = 0; i < rg.Count; i++)
            {
                if (rg[i] != ".")
                {
                    IntVec3 c = rect.Cells.ElementAt(i);
                    if (fullClean)
                    {
                        c.GetThingList(map).ToList().ForEach((t) =>
                        {
                            if (t.def.race == null)
                            {
                                t.DeSpawn();
                            }
                        });
                    }
                    else
                        c.GetThingList(map).ToList()
                                       .FindAll(t1 => (t1.def.category == ThingCategory.Filth) ||
                                                      (t1.def.thingCategories != null && t1.def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks)) ||
                                                      (t1.def.category == ThingCategory.Building && !t1.def.building.isNaturalRock))
                                       .ForEach((t) => t.DeSpawn());

                    if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
                    {
                        map.terrainGrid.SetTerrain(c, terrain);
                    }
                }
            }
        }

        public static void SimplePreClean(Map map, CellRect rect, bool fullClean)
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

            foreach (IntVec3 c in rect)
            {
                if (fullClean)
                    c.GetThingList(map).ToList().ForEach((t) => t.DeSpawn());
                else
                    c.GetThingList(map).ToList()
                                   .FindAll(t1 => (t1.def.category == ThingCategory.Filth) ||
                                                  (t1.def.thingCategories != null && t1.def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks)) ||
                                                  (t1.def.category == ThingCategory.Building && !t1.def.building.isNaturalRock))
                                   .ForEach((t) => t.DeSpawn());

                if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
                {
                    map.terrainGrid.SetTerrain(c, terrain);
                }
            }
        }

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