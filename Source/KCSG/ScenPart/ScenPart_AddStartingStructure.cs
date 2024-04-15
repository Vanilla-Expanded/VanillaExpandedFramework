using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class ScenPart_AddStartingStructure : ScenPart
    {
        // Gen options
        public bool allowFoggedPosition = false;
        public List<StructureLayoutDef> chooseFrom = new List<StructureLayoutDef>();
        public bool nearMapCenter;
        public bool spawnPartOfEnnemyFaction = false;
        public bool spawnConduits = true;
        // Name in scenario
        public string structureLabel;
        // Clear options
        public bool preGenClear = true;
        public bool fullClear = false;
        public bool clearFogInRect = false;
        // Starting spawn spawning
        public bool spawnTheStartingPawn = false;
        public PlayerPawnsArriveMethod method = PlayerPawnsArriveMethod.Standing;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            var scenPartRect = listing.GetScenPartRect(this, RowHeight * 6);

            var labelRect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, RowHeight);
            structureLabel = Widgets.TextField(labelRect, structureLabel);

            var addRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight, scenPartRect.width, RowHeight);
            if (Widgets.ButtonText(addRect, "KCSG.Add".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (DefDatabase<StructureLayoutDef>.AllDefs.Count() > 0)
                {
                    foreach (var item in DefDatabase<StructureLayoutDef>.AllDefs)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.defName, () => chooseFrom.Add(item)));
                    }
                }
                else
                {
                    floatMenuOptions.Add(new FloatMenuOption("No structure available", null));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            var removeRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 2, scenPartRect.width, RowHeight);
            if (Widgets.ButtonText(removeRect, "KCSG.Remove".Translate()))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                if (chooseFrom.Count > 0)
                {
                    foreach (var item in chooseFrom)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(item.defName, () => chooseFrom.Remove(item)));
                    }
                }
                else
                {
                    floatMenuOptions.Add(new FloatMenuOption("Nothing to remove", null));
                }
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }

            // Generation options
            var nearCenterRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 3, scenPartRect.width, RowHeight);
            Widgets.CheckboxLabeled(nearCenterRect, "KCSG.SpawnNearCenter".Translate(), ref nearMapCenter);

            var foogedRect = new Rect(scenPartRect.x, scenPartRect.y + RowHeight * 4, scenPartRect.width, RowHeight);
            Widgets.CheckboxLabeled(foogedRect, "KCSG.AllowFoggedPosition".Translate(), ref allowFoggedPosition);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spawnConduits, "spawnConduits");
            Scribe_Values.Look(ref preGenClear, "preGenClear");
            Scribe_Values.Look(ref fullClear, "fullClear");
            Scribe_Values.Look(ref clearFogInRect, "clearFogInRect");
            Scribe_Values.Look(ref spawnTheStartingPawn, "spawnTheStartingPawn");
            Scribe_Values.Look(ref structureLabel, "structureLabel");
            Scribe_Values.Look(ref nearMapCenter, "nearMapCenter");
            Scribe_Values.Look(ref allowFoggedPosition, "allowFoggedPosition");
            Scribe_Values.Look(ref spawnPartOfEnnemyFaction, "spawnPartOfEnnemyFaction");
            Scribe_Collections.Look(ref chooseFrom, "chooseFrom", LookMode.Def);
        }

        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "MapContain")
            {
                yield return structureLabel.CapitalizeFirst();
            }
            yield break;
        }

        public override void PostMapGenerate(Map map)
        {
            if (Find.TickManager.TicksGame > 5f || chooseFrom.Count <= 0 || PrepareCarefully_Util.pcScenariosSave.Count <= 0) return;

            StructureLayoutDef layoutDef;
            if (ModLister.GetActiveModWithIdentifier("EdB.PrepareCarefully") != null)
            {
                layoutDef = PrepareCarefully_Util.pcScenariosSave.First().Key;
                nearMapCenter = PrepareCarefully_Util.pcScenariosSave.First().Value;
            }
            else
            {
                layoutDef = chooseFrom.RandomElement();
            }

            CellRect cellRect = CreateCellRect(map, layoutDef.sizes.z, layoutDef.sizes.x);

            if (preGenClear)
                LayoutUtils.CleanRect(layoutDef, map, cellRect, fullClear);

            GenOption.GetAllMineableIn(cellRect, map);

            layoutDef.Generate(cellRect, map);

            if (spawnTheStartingPawn && Find.GameInitData != null)
            {
                List<List<Thing>> thingsGroups = new List<List<Thing>>();
                foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
                {
                    thingsGroups.Add(new List<Thing>() { startingAndOptionalPawn });
                }

                List<Thing> thingList = new List<Thing>();
                foreach (ScenPart allPart in Find.Scenario.AllParts)
                {
                    thingList.AddRange(allPart.PlayerStartingThings());
                }

                int index = 0;
                foreach (Thing thing in thingList)
                {
                    if (thing.def.CanHaveFaction)
                    {
                        thing.SetFaction(Faction.OfPlayer);
                    }

                    thingsGroups[index].Add(thing);
                    ++index;
                    if (index >= thingsGroups.Count)
                    {
                        index = 0;
                    }
                }

                var center = map.Center;
                var offset = layoutDef.spawnAt.RandomElement();
                center.x += offset.x;
                center.y += offset.z;
                Debug.Message($"Spawning pawns and stuff at {center}");
                DropThingGroupsAt(center, map, thingsGroups, instaDrop: (Find.GameInitData.QuickStarted || method != PlayerPawnsArriveMethod.DropPods), leaveSlag: true, allowFogged: false);
            }

            if (map.mapPawns.FreeColonistsSpawned.Count > 0)
            {
                FloodFillerFog.DebugRefogMap(map);
            }
            // Clear fog in rect if wanted
            if (clearFogInRect)
            {
                foreach (var c in cellRect)
                {
                    if (map.fogGrid.IsFogged(c))
                        map.fogGrid.Unfog(c);
                    else
                        MapGenerator.rootsToUnfog.Add(c);
                }
            }
        }

        private void DropThingGroupsAt(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool forbid = true, bool allowFogged = true)
        {
            foreach (List<Thing> thingsGroup in thingsGroups)
            {
                if (forbid)
                {
                    for (int index = 0; index < thingsGroup.Count; ++index)
                        thingsGroup[index].SetForbidden(true, false);
                }
                if (instaDrop)
                {
                    foreach (Thing thing in thingsGroup)
                        GenPlace.TryPlaceThing(thing, dropCenter, map, ThingPlaceMode.Near);
                }
                else
                {
                    ActiveDropPodInfo info = new ActiveDropPodInfo();
                    foreach (Thing thing in thingsGroup)
                        info.innerContainer.TryAdd(thing);
                    info.openDelay = openDelay;
                    info.leaveSlag = leaveSlag;
                    DropPodUtility.MakeDropPodAt(dropCenter, map, info);
                }
            }
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, "MapContain", "KCSG.Mapcontains".Translate());
        }

        private bool CanPlaceInRange(CellRect rect, Map map)
        {
            foreach (IntVec3 c in rect.Cells)
            {
                if (c.InBounds(map))
                {
                    TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
                    if (terrainDef.HasTag("River"))
                    {
                        return false;
                    }
                    if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.Wall, c, map, Rot4.North, null, ThingDefOf.Granite))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CanScatterAt(IntVec3 c, Map map, int height, int widht)
        {
            if (c.CloseToEdge(map, height)) return false;
            if (!allowFoggedPosition && c.Fogged(map)) return false;
            if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy)) return false;
            CellRect rect = new CellRect(c.x, c.z, widht, height).ClipInsideMap(map);
            return CanPlaceInRange(rect, map);
        }

        private CellRect CreateCellRect(Map map, int height, int widht)
        {
            IntVec3 rectCenter;
            if (nearMapCenter) rectCenter = map.Center;
            else rectCenter = map.AllCells.ToList().FindAll(c => CanScatterAt(c, map, height, widht)).InRandomOrder().RandomElement();

            return CellRect.CenteredOn(rectCenter, widht, height);
        }
    }
}