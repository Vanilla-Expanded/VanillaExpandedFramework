using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
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
                        else if (temp.pawnKindDefNS != null && (GenOption.ext == null || GenOption.ext.shouldRuin == false))
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
                                if (GenOption.ext?.shouldRuin == true)
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
            var pawns = new List<Pawn>();

            for (int i = 0; i < symbol.numberToSpawn; i++)
            {
                Pawn pawn = symbol.spawnPartOfFaction ? PawnGenerator.GeneratePawn(symbol.pawnKindDefNS, map.ParentFaction) : PawnGenerator.GeneratePawn(symbol.pawnKindDefNS, symbol.faction != null ? Find.FactionManager.FirstFactionOfDef(symbol.faction) : null);
                if (pawn == null)
                {
                    KLog.Message("Null pawn in GeneratePawnAt");
                    break;
                }

                if (symbol.isSlave && parentFaction)
                {
                    pawn.guest.SetGuestStatus(map.ParentFaction, GuestStatus.Prisoner);
                }

                if (symbol.spawnDead)
                {
                    pawn.Kill(new DamageInfo(DamageDefOf.Cut, 9999));
                    Corpse corpse = pawn.Corpse;
                    corpse.timeOfDeath = Mathf.Max(Find.TickManager.TicksGame - 120000, 0);
                    if (symbol.spawnRotten)
                    {
                        corpse.timeOfDeath = Mathf.Max(Find.TickManager.TicksGame - 60000 * Rand.RangeInclusive(5, 15), 0);
                        corpse.TryGetComp<CompRottable>().RotImmediately();
                        if (symbol.spawnFilthAround)
                        {
                            for (int x = 0; x < 5; x++)
                            {
                                IntVec3 rNext = new IntVec3();
                                RCellFinder.TryFindRandomCellNearWith(cell, ni => ni.Walkable(map), map, out rNext, 1, 3);
                                GenSpawn.Spawn(ThingDefOf.Filth_CorpseBile, rNext, map);
                            }
                        }
                    }
                    GenSpawn.Spawn(corpse, cell, map);
                }
                else
                {
                    GenSpawn.Spawn(pawn, cell, map, WipeMode.FullRefund);
                    pawns.Add(pawn);
                }
            }

            if (symbol.defendSpawnPoint)
            {
                Lord lord = LordMaker.MakeNewLord(map.ParentFaction, new LordJob_DefendPoint(cell, 3f, addFleeToil: false), map, pawns);
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
            if (layout.roofGrid != null && layout.roofGridResolved.Count > 0)
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

        /// <summary>
        /// Get road type already on map if applicable
        /// </summary>
        public static void SetRoadInfo(Map map)
        {
            if (map.TileInfo?.Roads?.Count > 0)
            {
                GenOption.preRoadTypes = new List<TerrainDef>();
                foreach (RimWorld.Planet.Tile.RoadLink roadLink in map.TileInfo.Roads)
                {
                    foreach (RoadDefGenStep rgs in roadLink.road.roadGenSteps)
                    {
                        if (rgs is RoadDefGenStep_Place rgsp && rgsp != null && rgsp.place is TerrainDef t && t != null && t != TerrainDefOf.Bridge)
                        {
                            GenOption.preRoadTypes.Add(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clean structure spawn rect. Full clean remove everything but player/map pawns.
        /// Normal clean remove filth, non-natural buildings and stone chunks
        /// </summary>
        public static void PreClean(Map map, CellRect rect, bool fullClean, List<string> roofGrid = null)
        {
            KLog.Message($"Pre-generation map clean. Fullclean {fullClean}");
            var mapFaction = map.ParentFaction;
            var player = Faction.OfPlayer;

            if (roofGrid != null)
            {
                var cells = rect.Cells.ToList();
                for (int i = 0; i < roofGrid.Count; i++)
                {
                    IntVec3 cell = cells[i];
                    if (cell.InBounds(map) && roofGrid[i] != ".")
                        CleanAt(cell, map, fullClean, mapFaction, player);
                }
            }
            else
            {
                foreach (IntVec3 c in rect)
                    CleanAt(c, map, fullClean, mapFaction, player);
            }
        }

        /// <summary>
        /// Clean at a cell. Terrain & things
        /// </summary>
        public static void CleanAt(IntVec3 c, Map map, bool fullClean, Faction mapFaction, Faction player)
        {
            // Clean things
            var things = c.GetThingList(map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Thing thing && thing.Spawned)
                {
                    if (thing is Pawn p && p.Faction != mapFaction && p.Faction != player)
                    {
                        thing.DeSpawn();
                    }
                    if (fullClean)
                    {
                        thing.DeSpawn();
                    }
                    // Clear filth, buildings, stone chunks
                    else if (thing.def.category == ThingCategory.Filth ||
                             (thing.def.category == ThingCategory.Building && thing.def.building.isNaturalRock) ||
                             (thing.def.thingCategories != null && thing.def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks)))
                    {
                        thing.DeSpawn();
                    }
                }
            }

            // Clean terrain
            if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
            {
                map.terrainGrid.SetTerrain(c, terrain);
            }
        }
    }
}