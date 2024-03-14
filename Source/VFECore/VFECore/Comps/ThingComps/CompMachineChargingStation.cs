using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Buildings;
using VFE.Mechanoids.Needs;
using VFECore;

namespace VFE.Mechanoids
{
    public class CompMachineChargingStation : CompPawnDependsOn
    {
        public bool wantsRespawn=false; //Used to determine whether a rebuild job is desired
        public bool forceStay = false;
        public bool wantsRest = false; //Used to force a machine to return to base, for healing or recharging
        public Area allowedArea = null;
        public bool energyDrainMode = true;

        public static List<CompMachineChargingStation> cachedChargingStations = new List<CompMachineChargingStation>();
        public static Dictionary<Thing, CompMachineChargingStation> cachedChargingStationsDict = new Dictionary<Thing, CompMachineChargingStation>();
        private CompPowerTrader compPower;
        public CompPowerTrader PowerComp
        {
            get
            {
                if (compPower is null) 
                {
                    compPower = parent.TryGetComp<CompPowerTrader>();
                }
                return compPower;
            }
        }
        public new CompProperties_MachineChargingStation Props
        {
            get
            {
                return (CompProperties_MachineChargingStation)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            cachedChargingStations.Add(this);
            cachedChargingStationsDict.Add(this.parent, this);
            if (!respawningAfterLoad)
                SpawnMyPawn();
            else
                CheckWantsRespawn();
            if (this.myPawn != null && this.myPawn.Position == this.parent.Position && this.myPawn.needs.TryGetNeed<Need_Power>().CurLevel >= 0.99f)
            {
                this.StopEnergyDrain();
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            cachedChargingStations.Remove(this);
            cachedChargingStationsDict.Remove(this.parent);
        }

        public override void SpawnMyPawn()
        {
            base.SpawnMyPawn();
            if (myPawn.story == null)
                myPawn.story = new Pawn_StoryTracker(myPawn);
            if (myPawn.skills == null)
                myPawn.skills = new Pawn_SkillTracker(myPawn);
            if (myPawn.workSettings == null)
                myPawn.workSettings = new Pawn_WorkSettings(myPawn);
            if (myPawn.relations == null)
                myPawn.relations = new Pawn_RelationsTracker(myPawn);
            DefMap<WorkTypeDef, int> priorities = new DefMap<WorkTypeDef, int>();
            priorities.SetAll(0);
            typeof(Pawn_WorkSettings).GetField("priorities", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(myPawn.workSettings, priorities);
            
            if (myPawn.def.race.mechEnabledWorkTypes != null)
            {
                foreach (WorkTypeDef workType in myPawn.def.race.mechEnabledWorkTypes)
                {
                    foreach (SkillDef skill in workType.relevantSkills)
                    {
                        SkillRecord record = myPawn.skills.skills.Find(rec => rec.def == skill);
                        record.levelInt = Props.skillLevel;
                    }
                    myPawn.workSettings.SetPriority(workType, 1);
                }
            }

            if (myPawn.TryGetComp<CompMachine>().Props.violent)
            {
                if (myPawn.drafter == null)
                    myPawn.drafter = new Pawn_DraftController(myPawn);
                if (Props.spawnWithWeapon != null)
                {
                    ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(Props.spawnWithWeapon);
                    myPawn.equipment.AddEquipment(thing);
                }
            }
            if (myPawn.needs.TryGetNeed<Need_Power>() == null)
                typeof(Pawn_NeedsTracker).GetMethod("AddNeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(myPawn.needs, new object[] { DefDatabase<NeedDef>.GetNamed("VFE_Mechanoids_Power") });
            myPawn.needs.TryGetNeed<Need_Power>().CurLevel = 0;

            if (myPawn.playerSettings != null)
            {
                myPawn.playerSettings.AreaRestrictionInPawnCurrentMap = allowedArea;
            }
            wantsRespawn = false;
        }



        public override void CompTickRare()
        {
            base.CompTickRare();
            IBedMachine bed = (IBedMachine)parent;
            var consumption = PowerComp.Props.PowerConsumption;

            if(bed.occupant!=null)
            {
                if (this.energyDrainMode)
                {
                    PowerComp.powerOutputInt = 0 - consumption - Props.extraChargingPower;
                }
                if (myPawn.health.hediffSet.HasNaturallyHealingInjury() && parent.TryGetComp<CompPowerTrader>().PowerOn)
                {
                    float num3 = 12f;

                    var hediffsInjury = new List<Hediff_Injury>();
                    myPawn.health.hediffSet.GetHediffs<Hediff_Injury>(ref hediffsInjury);
                    (from x in hediffsInjury where x.CanHealNaturally() select x).RandomElement().Heal(num3 * myPawn.HealthScale * 0.01f);
                }
            }
            else if (this.energyDrainMode)
            {
                PowerComp.powerOutputInt = 0 - consumption;
            }
            CheckWantsRespawn();
        }

        void CheckWantsRespawn()
        {
            if (!MyPawnIsAlive)
                wantsRespawn = true;
            else
                wantsRespawn = false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref wantsRest, "wantsRest");
            Scribe_Values.Look<bool>(ref forceStay, "forceStay");
            Scribe_Values.Look<bool>(ref energyDrainMode, "energyDrainMode", true);
            Scribe_References.Look<Area>(ref allowedArea, "allowedArea");
        }

        public void StopEnergyDrain()
        {
            if (myPawn.needs.TryGetNeed<Need_Power>().CurLevel >= 0.99f)
            {
                PowerComp.powerOutputInt = -1;
                this.energyDrainMode = false;
            }
        }

        public void StartEnergyDrain()
        {
            if (myPawn.needs.TryGetNeed<Need_Power>().CurLevel < 0.99f)
            {
                this.energyDrainMode = true;
                PowerComp.powerOutputInt = -PowerComp.Props.PowerConsumption;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            Command_Toggle forceRest = new Command_Toggle
            {
                defaultLabel = "VFEMechForceRecharge".Translate(),
                defaultDesc = "VFEMechForceRechargeDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/ForceRecharge"),
                toggleAction = delegate 
                {
                    foreach (var t in Find.Selector.SelectedObjects)
                    {
                        if (t is ThingWithComps thing && thing.TryGetComp<CompMachineChargingStation>() is CompMachineChargingStation comp)
                        {
                            if (comp.forceStay)
                            {
                                comp.wantsRest = false;
                                comp.forceStay = false;
                            }
                            else
                            {
                                comp.wantsRest = true;
                                comp.forceStay = true;
                                Job job = JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, comp.parent);
                                comp.myPawn.jobs.StopAll();
                                comp.myPawn.jobs.TryTakeOrderedJob(job);
                            }
                        }
                    }
                }
            };
            forceRest.isActive = delegate { return forceStay; };
            yield return forceRest;

            if (Props.showSetArea)
            {
                Command_Action setArea = new Command_Action
                {
                    defaultLabel = "VFEMechSetArea".Translate(),
                    defaultDesc = "VFEMechSetAreaDesc".Translate(),
                    action = delegate
                    {
                        AreaUtility.MakeAllowedAreaListFloatMenu(delegate (Area area)
                        {
                            foreach (var t in Find.Selector.SelectedObjects)
                            {
                                if (t is ThingWithComps thing && thing.TryGetComp<CompMachineChargingStation>() is CompMachineChargingStation comp)
                                {
                                    comp.allowedArea = area;
                                    if (comp.myPawn != null && !comp.myPawn.Dead)
                                    {
                                        comp.myPawn.playerSettings.AreaRestrictionInPawnCurrentMap = area;
                                    }
                                }
                            }

                        }, true, true, parent.Map);
                    },
                    icon = ContentFinder<Texture2D>.Get("UI/SelectZone")
                };
                yield return setArea;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder builder = new StringBuilder(base.CompInspectStringExtra());
            if (Props.pawnToSpawn != null)
            {
                if (myPawn == null || myPawn.Dead)
                {
                    bool comma = false;
                    string resources = "VFEMechReconstruct".Translate() + " ";
                    foreach (ThingDefCountClass resource in Props.pawnToSpawn.race.butcherProducts)
                    {
                        if (comma)
                            resources += ", ";
                        comma = true;
                        resources += resource.thingDef.label + " x" + resource.count;
                    }
                    builder.AppendLine(resources);
                }
            }
            return builder.ToString().Trim();
        }
    }
}
