using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace VFECore
{

    public class LordToil_SiegeCustom : LordToil
    {

        private const float BaseRadiusMin = 14f;
        private const float BaseRadiusMax = 25f;
        private static readonly FloatRange NutritionRangePerRaider = new FloatRange(0.9f, 2.7f);
        private const int StartBuildingDelay = 450;
        private static readonly FloatRange BuilderCountFraction = new FloatRange(0.25f, 0.4f);
        private const float FractionLossesToAssault = 0.4f;
        private const int InitalShellsPerCannon = 5;
        private const int ReplenishAtShells = 4;
        private const int ShellReplenishCount = 10;
        private const int ReplenishAtMeals = 5;
        private const int MealReplenishCount = 12;

        public Dictionary<Pawn, DutyDef> rememberedDuties = new Dictionary<Pawn, DutyDef>();

        public LordToil_SiegeCustom(IntVec3 siegeCenter, float blueprintPoints)
        {
            data = new LordToilData_Siege();
            Data.siegeCenter = siegeCenter;
            Data.blueprintPoints = blueprintPoints;
        }

        public override IntVec3 FlagLoc => Data.siegeCenter;

        private LordToilData_Siege Data => (LordToilData_Siege)data;

        private FactionDefExtension FactionDefExtension => FactionDefExtension.Get(lord.faction.def);

        private IEnumerable<Frame> Frames
        {
            get
            {
                LordToilData_Siege data = Data;
                float radSquared = (data.baseRadius + 10f) * (data.baseRadius + 10f);
                List<Thing> framesList = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
                if (framesList.Count == 0)
                {
                    yield break;
                }
                for (int i = 0; i < framesList.Count; i++)
                {
                    Frame frame = (Frame)framesList[i];
                    if (frame.Faction == lord.faction && (float)(frame.Position - data.siegeCenter).LengthHorizontalSquared < radSquared)
                    {
                        yield return frame;
                    }
                }
                yield break;
            }
        }

        public override bool ForceHighStoryDanger => true;

        public override void Init()
        {
            base.Init();

            var factionDefExtension = FactionDefExtension;

            Data.baseRadius = Mathf.InverseLerp(BaseRadiusMin, BaseRadiusMax, (float)lord.ownedPawns.Count / 50);
            Data.baseRadius = Mathf.Clamp(Data.baseRadius, BaseRadiusMin, BaseRadiusMax);
            List<Thing> list = new List<Thing>();
            foreach (Blueprint_Build blueprint_Build in SiegeBlueprintPlacer.PlaceBlueprints(Data.siegeCenter, base.Map, lord.faction, Data.blueprintPoints))
            {
                Data.blueprints.Add(blueprint_Build);
                using (List<ThingDefCountClass>.Enumerator enumerator2 = blueprint_Build.MaterialsNeeded().GetEnumerator())
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
                ThingDef thingDef = blueprint_Build.def.entityDefToBuild as ThingDef;
                if (thingDef != null)
                {
                    ThingDef turret = thingDef;
                    bool allowEMP = false;
                    TechLevel techLevel = lord.faction.def.techLevel;
                    ThingDef thingDef2 = TurretGunUtility.TryFindRandomShellDef(turret, allowEMP, true, techLevel, false, 250f);
                    if (thingDef2 != null)
                    {
                        Thing thing3 = ThingMaker.MakeThing(thingDef2, null);
                        thing3.stackCount = 5;
                        list.Add(thing3);
                    }
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].stackCount = Mathf.CeilToInt((float)list[i].stackCount * Rand.Range(1f, 1.2f));
            }
            List<List<Thing>> list2 = new List<List<Thing>>();
            for (int j = 0; j < list.Count; j++)
            {
                while (list[j].stackCount > list[j].def.stackLimit)
                {
                    int num = Mathf.CeilToInt((float)list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
                    Thing thing4 = ThingMaker.MakeThing(list[j].def, null);
                    thing4.stackCount = num;
                    list[j].stackCount -= num;
                    list.Add(thing4);
                }
            }
            List<Thing> list3 = new List<Thing>();
            for (int k = 0; k < list.Count; k++)
            {
                list3.Add(list[k]);
                if (k % 2 == 1 || k == list.Count - 1)
                {
                    list2.Add(list3);
                    list3 = new List<Thing>();
                }
            }
            List<Thing> list4 = new List<Thing>();
            int num2 = Mathf.RoundToInt(NutritionRangePerRaider.RandomInRange / factionDefExtension.siegeMealDef.GetStatValueAbstract(StatDefOf.Nutrition) * lord.ownedPawns.Count);
            for (int l = 0; l < num2; l++)
            {
                Thing item = ThingMaker.MakeThing(factionDefExtension.siegeMealDef, null);
                list4.Add(item);
            }
            list2.Add(list4);
            DropPodUtility.DropThingGroupsNear(Data.siegeCenter, Map, list2, 110);
            Data.desiredBuilderFraction = BuilderCountFraction.RandomInRange;
        }

        public override void UpdateAllDuties()
        {
            LordToilData_Siege data = Data;
            if (lord.ticksInToil < 450)
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    SetAsDefender(lord.ownedPawns[i]);
                }
            }
            else
            {
                rememberedDuties.Clear();
                int num = Mathf.RoundToInt((float)lord.ownedPawns.Count * data.desiredBuilderFraction);
                if (num <= 0)
                {
                    num = 1;
                }
                int num2 = (from b in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
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
                    Pawn pawn2;
                    if ((from pa in lord.ownedPawns
                         where !rememberedDuties.ContainsKey(pa) && CanBeBuilder(pa)
                         select pa).TryRandomElement(out pawn2))
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
            return !p.story.WorkTypeIsDisabled(WorkTypeDefOf.Construction) && !p.story.WorkTypeIsDisabled(WorkTypeDefOf.Firefighter);
        }

        private void SetAsBuilder(Pawn p)
        {
            LordToilData_Siege data = Data;
            var factionDefExtension = FactionDefExtension;
            p.mindState.duty = new PawnDuty(DutyDefOf.Build, data.siegeCenter, -1f);
            p.mindState.duty.radius = data.baseRadius;
            int minLevel = Mathf.Max(RimWorld.ThingDefOf.Sandbags.constructionSkillPrerequisite, ThingDefOf.Turret_Mortar.constructionSkillPrerequisite);
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
            LordToilData_Siege data = Data;
            p.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.siegeCenter, -1f);
            p.mindState.duty.radius = data.baseRadius;
        }

        public override void LordToilTick()
        {
            base.LordToilTick();
            var factionDefExtension = FactionDefExtension;
            LordToilData_Siege data = Data;
            if (lord.ticksInToil == 450)
            {
                lord.CurLordToil.UpdateAllDuties();
            }
            if (lord.ticksInToil > 450 && lord.ticksInToil % 500 == 0)
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
                          select blue).Any<Blueprint>() && !base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any((Thing b) => b.Faction == lord.faction && b.def.building.buildingTags.Contains("Artillery")))
                    {
                        lord.ReceiveMemo("NoArtillery");
                        return;
                    }
                }
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
                            if (thingList[j].def.IsShell)
                            {
                                shellCount += thingList[j].stackCount;
                            }
                            if (thingList[j].def == ThingDefOf.MealSurvivalPack)
                            {
                                num3 += thingList[j].stackCount;
                            }
                        }
                    }
                }
                if (shellCount < 4)
                {
                    ThingDef turret_Mortar = ThingDefOf.Turret_Mortar;
                    bool allowEMP = false;
                    TechLevel techLevel = lord.faction.def.techLevel;
                    ThingDef thingDef = TurretGunUtility.TryFindRandomShellDef(turret_Mortar, allowEMP, true, techLevel, false, 250f);
                    if (thingDef != null)
                    {
                        DropSupplies(thingDef, 10);
                    }
                }
                if (num3 < 5)
                {
                    DropSupplies(ThingDefOf.MealSurvivalPack, 12);
                }
            }
        }

        private void DropSupplies(ThingDef thingDef, int count)
        {
            List<Thing> list = new List<Thing>();
            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = count;
            list.Add(thing);
            DropPodUtility.DropThingsNear(Data.siegeCenter, base.Map, list, 110, false, false, true);
        }

        public override void Cleanup()
        {
            LordToilData_Siege data = Data;
            data.blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
            for (int i = 0; i < data.blueprints.Count; i++)
            {
                data.blueprints[i].Destroy(DestroyMode.Cancel);
            }
            foreach (Frame frame in Frames.ToList<Frame>())
            {
                frame.Destroy(DestroyMode.Cancel);
            }
        }

    }

}
