using AnimalBehaviours;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFE.Mechanoids.Needs;
using VFECore;

namespace VFE.Mechanoids
{
    public class CompMachine : CompDependsOnBuilding, PawnGizmoProvider
    {
        public ThingDef turretToInstall = null; //Used to specify a turret to put on the mobile turret
        public ThingDef turretAttached = null;
        public float turretAngle = 0f; //Purely cosmetic, don't need to save it
        public float turretAnglePerFrame = 0.1f;

        public static Dictionary<CompMachine, Pawn> cachedPawns = new Dictionary<CompMachine, Pawn>();
        public static Dictionary<Pawn, CompMachine> cachedMachinesPawns = new Dictionary<Pawn, CompMachine>();
        public override void OnBuildingDestroyed(CompPawnDependsOn compPawnDependsOn)
        {
            base.OnBuildingDestroyed(compPawnDependsOn);
            if (compPawnDependsOn.Props.killPawnAfterDestroying)
            {
                parent.Kill();
            }
        }

        public new CompProperties_Machine Props => this.props as CompProperties_Machine;

        public CompProperties_MachineChargingStation StationProps
        {
            get
            {
                var comp = this.myBuilding?.GetComp<CompMachineChargingStation>();
                if (comp != null)
                {
                    return comp.Props;
                }
                return null;
            }
        }
        public void AttachTurret()
        {
            if(turretAttached!=null)
            {
                foreach(ThingDefCountClass stack in turretAttached.costList)
                {
                    Thing thing = ThingMaker.MakeThing(stack.thingDef);
                    thing.stackCount = stack.count;
                    GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
                }
                ((Pawn)parent).equipment.DestroyAllEquipment();
            }
            turretAttached = turretToInstall;
            Thing turretThing = ThingMaker.MakeThing(turretToInstall.building.turretGunDef);
            ((Pawn)parent).equipment.AddEquipment((ThingWithComps)turretThing);
            turretToInstall = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<ThingDef>(ref turretAttached, "turretAttached");
            Scribe_Defs.Look<ThingDef>(ref turretToInstall, "turretToInstall");
        }

        public override void CompTick()
        {
            base.CompTick();
            if(turretAttached!=null)
            {
                turretAngle += turretAnglePerFrame;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            turretAnglePerFrame = Rand.Range(-0.5f, 0.5f);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            cachedPawns.Add(this, (Pawn)parent);
            cachedMachinesPawns.Add((Pawn)parent, this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            cachedPawns.Remove(this);
            cachedMachinesPawns.Remove((Pawn)parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            cachedPawns.Remove(this);
            cachedMachinesPawns.Remove((Pawn)parent);
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Recharge fully",
                    action = delegate ()
                    {
                        (this.parent as Pawn).needs.TryGetNeed<Need_Power>().CurLevel = 1;
                    }
                };
            }
            if (CanHaveTurret())
            {
                Command_Action attachTurret = new Command_Action
                {
                    defaultLabel = "VFEMechAttachTurret".Translate(),
                    defaultDesc = "VFEMechAttachTurretDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/AttachTurret"),
                    action = delegate {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs.Where(t => IsTurretAllowed(t)))
                        {
                            FloatMenuOption opt = new FloatMenuOption(thing.label, delegate
                            {
                                turretToInstall = thing;
                                var comp = this.myBuilding.GetComp<CompMachineChargingStation>();
                                if (comp != null)
                                {
                                    comp.wantsRest = true;
                                }
                            }, thing.building.turretGunDef);
                            options.Add(opt);
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
                yield return attachTurret;
            }
        }

        public override string CompInspectStringExtra()
        {
            var builder = new StringBuilder(base.CompInspectStringExtra());
            if (turretToInstall != null)
            {
                bool comma = false;
                string resources = "VFEMechTurretResources".Translate() + " ";
                foreach (ThingDefCountClass resource in turretToInstall.costList)
                {
                    if (comma)
                        resources += ", ";
                    comma = true;
                    resources += resource.thingDef.label + " x" + resource.count;
                }
                builder.AppendLine(resources);
            }
            return builder.ToString().TrimEndNewlines();
        }

        public bool IsTurretAllowed(ThingDef t)
        {
            if (t.building != null && t.building.turretGunDef != null && t.costList != null 
                && t.GetCompProperties<CompProperties_Mannable>() == null && t.size.x <= 3 && t.size.z <= 3 && t.IsResearchFinished)
            {
                var stationProps = StationProps;
                if (stationProps?.blackListTurretGuns != null && stationProps.blackListTurretGuns.Contains(t.building.turretGunDef.defName))
                {
                    return false;
                }
                if (Props.blackListTurretGuns != null && Props.blackListTurretGuns.Contains(t.building.turretGunDef.defName))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool CanHaveTurret()
        {
            var props = StationProps;
            if (props != null && props.turret)
            {
                return true;
            }
            return Props.canUseTurrets;
        }
    }
}
