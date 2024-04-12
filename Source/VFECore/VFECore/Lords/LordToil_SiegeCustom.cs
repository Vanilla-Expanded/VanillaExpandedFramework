using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFECore
{
    public class LordToil_SiegeCustom : LordToil
    {

        private const float BaseRadiusMin = 14f;
        private const float BaseRadiusMax = 25f;
        private static readonly FloatRange NutritionRangePerRaider = new(0.9f, 2.7f);
        private const int StartBuildingDelay = 450;
        private static readonly FloatRange BuilderCountFraction = new(0.25f, 0.4f);
        private const int InitalShellsPerCannon = 5;
        private const int ReplenishAtShells = 4;
        private const int ShellReplenishCount = 10;
        private const int ReplenishAtMeals = 5;
        private const int MealReplenishCount = 12;

        public Dictionary<Pawn, DutyDef> rememberedDuties = new();

        public LordToil_SiegeCustom(IntVec3 siegeCenter, float blueprintPoints)
        {
            data = new LordToilData_SiegeCustom();
            Data.siegeCenter = siegeCenter;
            Data.blueprintPoints = blueprintPoints;
        }

        public override IntVec3 FlagLoc => Data.siegeCenter;

        private LordToilData_SiegeCustom Data => (LordToilData_SiegeCustom)data;

        private SiegeParameterSetDef CustomParams => FactionDefExtension.Get(lord.faction.def).siegeParameterSetDef;

        private IEnumerable<Frame> Frames
        {
            get
            {
                var data = Data;
                float radSquared = (data.baseRadius + 10f) * (data.baseRadius + 10f);
                List<Thing> framesList = Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
                if (framesList.Count == 0)
                {
                    yield break;
                }
                for (int i = 0; i < framesList.Count; i++)
                {
                    Frame frame = (Frame)framesList[i];
                    if (frame.Faction == lord.faction && (frame.Position - data.siegeCenter).LengthHorizontalSquared < radSquared)
                    {
                        yield return frame;
                    }
                }
                yield break;
            }
        }

        private IEnumerable<Building_TurretGun> Artillery => Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
            .Where(b => b.Faction == lord.faction && b.Position.InHorDistOf(FlagLoc, Data.baseRadius) && b.def.building.IsMortar).Cast<Building_TurretGun>();

        public override bool ForceHighStoryDanger => true;

        public override void Init()
        {
            base.Init();

            var customParams = CustomParams;

            Data.baseRadius = Mathf.InverseLerp(BaseRadiusMin, BaseRadiusMax, (float)lord.ownedPawns.Count / 50);
            Data.baseRadius = Mathf.Clamp(Data.baseRadius, BaseRadiusMin, BaseRadiusMax);
            List<Thing> list = new();
            var placedBlueprints = CustomSiegeUtility.PlaceBlueprints(Data, Map, lord.faction).ToList();
            for (int i = 0; i < placedBlueprints.Count; i++)
            {
                var blueprint_Build = placedBlueprints[i];
                Data.blueprints.Add(blueprint_Build);
                using (List<ThingDefCountClass>.Enumerator enumerator2 = blueprint_Build.TotalMaterialCost().GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        ThingDefCountClass cost = enumerator2.Current;
                        Thing thing = list.FirstOrDefault((Thing t) => t.def == cost.thingDef);
                        if (thing != null)
                        {
                            thing.stackCount += cost.count;
                        }
                        else
                        {
                            Thing thing2 = ThingMaker.MakeThing(cost.thingDef, null);
                            thing2.stackCount = cost.count;
                            list.Add(thing2);
                        }
                    }
                }

                if (blueprint_Build.def.entityDefToBuild is ThingDef thingDef)
                {
                    ThingDef turret = thingDef;
                    bool allowEMP = false;
                    TechLevel techLevel = lord.faction.def.techLevel;
                    if (TurretGunUtility.TryFindRandomShellDef(turret, allowEMP, false, true, techLevel, false, 250f) is ThingDef shellDef)
                    {
                        Thing thing3 = ThingMaker.MakeThing(shellDef, null);
                        thing3.stackCount = InitalShellsPerCannon;
                        list.Add(thing3);
                    }
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].stackCount = Mathf.CeilToInt(list[i].stackCount * Rand.Range(1f, 1.2f));
            }
            List<List<Thing>> list2 = new();
            for (int j = 0; j < list.Count; j++)
            {
                while (list[j].stackCount > list[j].def.stackLimit)
                {
                    int num = Mathf.CeilToInt(list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
                    Thing thing4 = ThingMaker.MakeThing(list[j].def, null);
                    thing4.stackCount = num;
                    list[j].stackCount -= num;
                    list.Add(thing4);
                }
            }
            List<Thing> list3 = new();
            for (int k = 0; k < list.Count; k++)
            {
                list3.Add(list[k]);
                if (k % 2 == 1 || k == list.Count - 1)
                {
                    list2.Add(list3);
                    list3 = new List<Thing>();
                }
            }
            List<Thing> list4 = new();
            int num2 = Mathf.RoundToInt(NutritionRangePerRaider.RandomInRange / customParams.mealDef.GetStatValueAbstract(StatDefOf.Nutrition) * lord.ownedPawns.Count);
            for (int l = 0; l < num2; l++)
            {
                Thing item = ThingMaker.MakeThing(customParams.mealDef, null);
                list4.Add(item);
            }
            list2.Add(list4);
            if (lord.faction.def.techLevel >= TechLevel.Industrial)
            {
                DropPodUtility.DropThingGroupsNear(Data.siegeCenter, Map, list2, 110);
            }
            else
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    var group = list2[i];
                    if (DropCellFinder.TryFindDropSpotNear(Data.siegeCenter, Map, out IntVec3 pos, false, false))
                    {
                        for (int j = 0; j < group.Count; j++)
                        {
                            var thing = group[j];
                            thing.SetForbidden(true, false);
                            GenPlace.TryPlaceThing(thing, pos, Map, ThingPlaceMode.Near);
                        }
                    }
                }
            }
            Data.desiredBuilderFraction = BuilderCountFraction.RandomInRange;
        }

        public override void UpdateAllDuties()
        {
            var data = Data;
            if (lord.ticksInToil < StartBuildingDelay)
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    SetAsDefender(lord.ownedPawns[i]);
                }
            }
            else
            {
                rememberedDuties.Clear();
                int num = Mathf.RoundToInt(lord.ownedPawns.Count * data.desiredBuilderFraction);
                if (num <= 0)
                {
                    num = 1;
                }
                int num2 = (from b in Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                            where b.def.hasInteractionCell && b.Faction == lord.faction && b.Position.InHorDistOf(FlagLoc, data.baseRadius)
                            select b).Count<Thing>();
                if (num < num2)
                {
                    num = num2;
                }
                int num3 = 0;
                for (int j = 0; j < lord.ownedPawns.Count; j++)
                {
                    Pawn pawn = lord.ownedPawns[j];
                    if (pawn.mindState.duty.def == DutyDefOf.Build)
                    {
                        rememberedDuties.Add(pawn, DutyDefOf.Build);
                        SetAsBuilder(pawn);
                        num3++;
                    }
                }
                int num4 = num - num3;
                for (int k = 0; k < num4; k++)
                {
                    if ((from pa in lord.ownedPawns
                         where !rememberedDuties.ContainsKey(pa) && CanBeBuilder(pa)
                         select pa).TryRandomElement(out Pawn pawn2))
                    {
                        rememberedDuties.Add(pawn2, DutyDefOf.Build);
                        SetAsBuilder(pawn2);
                        num3++;
                    }
                }
                for (int l = 0; l < lord.ownedPawns.Count; l++)
                {
                    Pawn pawn3 = lord.ownedPawns[l];
                    if (!rememberedDuties.ContainsKey(pawn3))
                    {
                        SetAsDefender(pawn3);
                        rememberedDuties.Add(pawn3, DutyDefOf.Defend);
                    }
                }
                if (num3 == 0)
                {
                    lord.ReceiveMemo("NoBuilders");
                    return;
                }
            }
        }

        public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
        {
            UpdateAllDuties();
            base.Notify_PawnLost(victim, cond);
        }

        public override void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
        {
            base.Notify_ConstructionFailed(pawn, frame, newBlueprint);
            if (frame.Faction == lord.faction && newBlueprint != null)
            {
                Data.blueprints.Add(newBlueprint);
            }
        }

        private bool CanBeBuilder(Pawn p)
        {
            return !p.WorkTypeIsDisabled(WorkTypeDefOf.Construction) && !p.WorkTypeIsDisabled(WorkTypeDefOf.Firefighter);
        }

        private void SetAsBuilder(Pawn p)
        {
            var data = Data;
            var customParams = CustomParams;
            p.mindState.duty = new PawnDuty(DutyDefOf.Build, data.siegeCenter, -1f)
            {
                radius = data.baseRadius
            };
            int minLevel = Mathf.Max(customParams.coverDef.constructionSkillPrerequisite, customParams.maxArtilleryConstructionSkill);
            p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(minLevel);
            p.workSettings.EnableAndInitialize();
            List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                WorkTypeDef workTypeDef = allDefsListForReading[i];
                if (workTypeDef == WorkTypeDefOf.Construction)
                {
                    p.workSettings.SetPriority(workTypeDef, 1);
                }
                else
                {
                    p.workSettings.Disable(workTypeDef);
                }
            }
        }

        private void SetAsDefender(Pawn p)
        {
            var data = Data;
            p.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.siegeCenter, -1f)
            {
                radius = data.baseRadius
            };
        }

        public override void LordToilTick()
        {
            base.LordToilTick();
            var customParams = CustomParams;
            var data = Data;
            if (lord.ticksInToil == StartBuildingDelay)
            {
                lord.CurLordToil.UpdateAllDuties();
            }
            if (lord.ticksInToil > StartBuildingDelay && lord.ticksInToil % 500 == 0)
            {
                UpdateAllDuties();
            }
            if (Find.TickManager.TicksGame % 500 == 0)
            {
                if (!(from frame in Frames
                      where !frame.Destroyed
                      select frame).Any<Frame>())
                {
                    if (!(from blue in data.blueprints
                          where !blue.Destroyed
                          select blue).Any<Blueprint>() && !Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any((Thing b) => b.Faction == lord.faction && b.def.building.buildingTags.Contains("Artillery")))
                    {
                        lord.ReceiveMemo("NoArtillery");
                        return;
                    }
                }
                var arties = Artillery;
                int shellCount = 0;
                int foodCount = 0;
                for (int i = 0; i < GenRadial.NumCellsInRadius(20); i++)
                {
                    IntVec3 c = data.siegeCenter + GenRadial.RadialPattern[i];
                    if (c.InBounds(Map))
                    {
                        List<Thing> thingList = c.GetThingList(Map);
                        for (int j = 0; j < thingList.Count; j++)
                        {
                            var curThing = thingList[j];
                            if (/*curThing.def.IsShell && */arties.Any(a => CustomSiegeUtility.AcceptsShell(a, curThing.def)))
                            {
                                shellCount += curThing.stackCount;
                            }
                            if (curThing.def == customParams.mealDef)
                            {
                                foodCount += curThing.stackCount;
                            }
                        }
                    }
                }
                // Prevent the shellpocalypse today!
                if (arties.Any() && shellCount < ReplenishAtShells)
                {
                    bool allowEMP = false;
                    var techLevel = lord.faction.def.techLevel;
                    var distinctArtillery = data.artilleryCounts.Keys.ToList();
                    var shellCountsToGive = new Dictionary<ThingDef, int>();
                    for (int i = 0; i < ShellReplenishCount; i++)
                    {
                        var artillery = distinctArtillery.RandomElementByWeight(a => data.artilleryCounts[a]);
                        ThingDef shellDef = TurretGunUtility.TryFindRandomShellDef(artillery, allowEMP, false, true, techLevel, false, 250f);
                        if (shellDef != null)
                        {
                            if (shellCountsToGive.ContainsKey(shellDef))
                                shellCountsToGive[shellDef]++;
                            else
                                shellCountsToGive.Add(shellDef, 1);
                        }
                    }
                    foreach (var shell in shellCountsToGive)
                        DropSupplies(shell.Key, shell.Value);
                }
                if (foodCount < FoodUtility.StackCountForNutrition(ReplenishAtMeals, customParams.mealDef.GetStatValueAbstract(StatDefOf.Nutrition)))
                {
                    DropSupplies(customParams.mealDef, FoodUtility.StackCountForNutrition(MealReplenishCount, customParams.mealDef.GetStatValueAbstract(StatDefOf.Nutrition)));
                }
            }
        }

        private void DropSupplies(ThingDef thingDef, int count)
        {
            List<Thing> list = new();
            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = count;
            list.Add(thing);
            if (lord.faction.def.techLevel >= TechLevel.Industrial)
            {
                DropPodUtility.DropThingsNear(Data.siegeCenter, Map, list, 110, false, false, true);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var t = list[i];
                    GenPlace.TryPlaceThing(t, Data.siegeCenter, Map, ThingPlaceMode.Near);
                }
            }
        }

        public override void Cleanup()
        {
            var data = Data;
            data.blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
            for (int i = 0; i < data.blueprints.Count; i++)
            {
                data.blueprints[i].Destroy(DestroyMode.Cancel);
            }
            var frameList = Frames.ToList();
            for (int i = 0; i < frameList.Count; i++)
            {
                var frame = frameList[i];
                frame.Destroy(DestroyMode.Cancel);
            }
        }

    }

}
