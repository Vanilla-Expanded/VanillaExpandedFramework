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
        /// <summary>
        /// Entry method to generate a structure from a StructureLayoutDef
        /// </summary>
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
                                if (cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                                {
                                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                                }

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
                                // Generating settlement, we want to keep tracks of doors
                                if (!GenOption.ext.useStructureLayout && temp.thingDef.altitudeLayer == AltitudeLayer.DoorMoveable)
                                {
                                    SettlementGenUtils.doors.Add(cell);
                                }

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

        /// <summary>
        /// Generate terrain at cell. Make bridge if needed, remove mineable if needed.
        /// </summary>
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

        /// <summary>
        /// Generate pawn(s) at pos
        /// </summary>
        public static void GeneratePawnAt(Map map, IntVec3 cell, SymbolDef symbol)
        {
            var factionManager = Find.FactionManager;
            var parentFaction = map.ParentFaction;
            var symbolFaction = symbol.faction != null ? factionManager.FirstFactionOfDef(symbol.faction) : null;
            var slaveFaction = factionManager.AllFactionsListForReading.Find(f => parentFaction.HostileTo(f)) ?? factionManager.AllFactionsListForReading.Find(f => f != Faction.OfPlayer && f != parentFaction);
            var pawns = new List<Pawn>();

            var request = new PawnGenerationRequest(symbol.pawnKindDefNS, symbol.isSlave ? slaveFaction : (symbol.spawnPartOfFaction ? parentFaction : symbolFaction), mustBeCapableOfViolence: true);

            for (int i = 0; i < symbol.numberToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (pawn == null)
                {
                    Debug.Message("Null pawn in GeneratePawnAt");
                    continue;
                }

                if (symbol.isSlave && parentFaction != null)
                {
                    pawn.guest.SetGuestStatus(parentFaction, GuestStatus.Prisoner);
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
                    GenSpawn.Spawn(pawn, cell, map, WipeMode.VanishOrMoveAside);
                    pawns.Add(pawn);
                }
            }

            if (symbol.defendSpawnPoint)
            {
                Lord lord = LordMaker.MakeNewLord(parentFaction, new LordJob_DefendPoint(cell, 3f, addFleeToil: false), map, pawns);
            }
        }

        /// <summary>
        /// Generate item at pos
        /// </summary>
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

        /// <summary>
        /// Generate building at pos
        /// </summary>
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
                List<Thing> innerThings = new List<Thing>();
                if (faction == Faction.OfPlayer && symbol.thingSetMakerDefForPlayer != null)
                {
                    innerThings = symbol.thingSetMakerDefForPlayer.root.Generate(new ThingSetMakerParams());
                }
                else if (symbol.thingSetMakerDef != null)
                {
                    innerThings = symbol.thingSetMakerDef.root.Generate(new ThingSetMakerParams());
                }

                for (int i = 0; i < innerThings.Count; i++)
                {
                    var innerThing = innerThings[i];
                    innerThing.stackCount = Math.Min((int)(innerThing.stackCount * symbol.crateStackMultiplier), innerThing.def.stackLimit);
                    if (!crate.TryAcceptThing(innerThing))
                    {
                        innerThing.Destroy();
                    }
                }
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

        /// <summary>
        /// Spawn mortar and manning pawn with the right job
        /// </summary>
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

        /// <summary>
        /// Generate pawn to be put inside container (casked, tomb...)
        /// </summary>
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

        /// <summary>
        /// Generate roof from layout resolved roof grid
        /// </summary>
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
                        var wantedRoof = layout.roofGridResolved[i];
                        if (wantedRoof == "0" && layout.forceGenerateRoof)
                        {
                            map.roofGrid.SetRoof(cell, null);
                            continue;
                        }

                        var currentRoof = cell.GetRoof(map);
                        if (wantedRoof == "1" && (layout.forceGenerateRoof || currentRoof == null))
                        {
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
                            continue;
                        }

                        if (wantedRoof == "2" && (layout.forceGenerateRoof || currentRoof == null || currentRoof == RoofDefOf.RoofConstructed))
                        {
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThin);
                            continue;
                        }

                        if (wantedRoof == "3")
                        {
                            map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                            continue;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Get road type already on map if applicable
        /// </summary>
        public static List<TerrainDef> SetRoadInfo(Map map)
        {
            if (map.TileInfo?.Roads?.Count > 0)
            {
                var preRoadTypes = new List<TerrainDef>();
                foreach (RimWorld.Planet.Tile.RoadLink roadLink in map.TileInfo.Roads)
                {
                    foreach (RoadDefGenStep rgs in roadLink.road.roadGenSteps)
                    {
                        if (rgs is RoadDefGenStep_Place rgsp && rgsp != null && rgsp.place is TerrainDef t && t != null && t != TerrainDefOf.Bridge)
                        {
                            preRoadTypes.Add(t);
                        }
                    }
                }
                return preRoadTypes;
            }
            return null;
        }

        /// <summary>
        /// Clean structure spawn rect. Full clean remove everything but player/map pawns.
        /// Normal clean remove filth, non-natural buildings and stone chunks
        /// </summary>
        public static void PreClean(Map map, CellRect rect, bool fullClean, List<string> roofGrid = null)
        {
            Debug.Message($"Pre-generation map clean. Fullclean {fullClean}");
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
                    else if (fullClean && (thing.def.category != ThingCategory.Building || !thing.def.building.isNaturalRock))
                    {
                        thing.DeSpawn();
                    }
                    // Clear filth, buildings, stone chunks
                    else if (thing.def.category == ThingCategory.Filth ||
                             (thing.def.category == ThingCategory.Building && !thing.def.building.isNaturalRock) ||
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

        /// <summary>
        /// Choose a random layout that match requirement(s) from a list.
        /// </summary>
        public static StructureLayoutDef ChooseStructureLayoutFrom(List<StructureLayoutDef> list)
        {
            List<StructureLayoutDef> choices = new List<StructureLayoutDef>();
            for (int i = 0; i < list.Count; i++)
            {
                var layout = list[i];
                if (layout.RequiredModLoaded)
                {
                    choices.Add(layout);
                }
            }
            return choices.RandomElement();
        }

        /// <summary>
        /// Choose a random WeightedStruct from a list.
        /// </summary>
        public static WeightedStruct ChooseWeightedStructFrom(List<WeightedStruct> list, IncidentParms parms)
        {
            List<WeightedStruct> choices = new List<WeightedStruct>();
            for (int i = 0; i < list.Count; i++)
            {
                var weightedStruct = list[i];
                if (weightedStruct.structureLayoutDef.RequiredModLoaded)
                {
                    choices.Add(weightedStruct);
                }
            }
            return choices.RandomElementByWeight((w) => w.weight * parms.points);
        }
    }
}