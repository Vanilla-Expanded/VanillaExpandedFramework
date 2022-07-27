using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VanillaFurnitureExpanded
{
    public class ColorModExtension : DefModExtension
    {
        public List<ColorOption> colorOptions = new List<ColorOption>();
    }
    public class ColorOption
    {
        public float overlightRadius;

        public float glowRadius = 14f;

        public string texPath;

        public ColorInt glowColor = new ColorInt(255, 255, 255, 0) * 1.45f;

        public string colorLabel = "";
    }
    public class CompProperties_GlowerExtended : CompProperties
    {
        public List<ColorOption> colorOptions;

        public bool spawnGlowerInFacedCell;
        public CompProperties_GlowerExtended()
        {
            compClass = typeof(CompGlowerExtended);
        }
    }

    public class CompGlowerExtended : ThingComp
    {
        private ColorOption currentColor;
        public int currentColorInd;
        public CompGlower compGlower;
        private bool dirty;
        public CompProperties_GlowerExtended Props => (CompProperties_GlowerExtended)props;
        public override string TransformLabel(string label)
        {
            if (!(currentColor?.colorLabel).NullOrEmpty())
            {
                return base.TransformLabel(label) + " (" + currentColor.colorLabel + ")";
            }
            return base.TransformLabel(label);
        }

        private bool ShouldBeLitNow
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                if (!FlickUtility.WantsToBeOn(parent))
                {
                    return false;
                }
                CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
                if (compPowerTrader != null && !compPowerTrader.PowerOn)
                {
                    return false;
                }
                CompRefuelable compRefuelable = parent.TryGetComp<CompRefuelable>();
                if (compRefuelable != null && !compRefuelable.HasFuel)
                {
                    return false;
                }
                CompSendSignalOnCountdown compSendSignalOnCountdown = parent.TryGetComp<CompSendSignalOnCountdown>();
                if (compSendSignalOnCountdown != null && compSendSignalOnCountdown.ticksLeft <= 0)
                {
                    return false;
                }
                CompSendSignalOnPawnProximity compSendSignalOnPawnProximity = parent.TryGetComp<CompSendSignalOnPawnProximity>();
                if (compSendSignalOnPawnProximity != null && compSendSignalOnPawnProximity.Sent)
                {
                    return false;
                }
                CompLoudspeaker compLoudspeaker = parent.TryGetComp<CompLoudspeaker>();
                if (compLoudspeaker != null && !compLoudspeaker.Active)
                {
                    return false;
                }
                CompHackable compHackable = parent.TryGetComp<CompHackable>();
                if (compHackable != null && compHackable.IsHacked && !compHackable.Props.glowIfHacked)
                {
                    return false;
                }
                CompRitualSignalSender compRitualSignalSender = parent.TryGetComp<CompRitualSignalSender>();
                if (compRitualSignalSender != null && !compRitualSignalSender.ritualTarget)
                {
                    return false;
                }
                Building_Crate building_Crate;
                if ((building_Crate = parent as Building_Crate) != null && !building_Crate.HasAnyContents)
                {
                    return false;
                }
                return true;
            }
        }

        public void UpdateLit()
        {
            bool shouldBeLitNow = ShouldBeLitNow;
            if (shouldBeLitNow)
            {
                this.UpdateGlower(currentColorInd);
                this.ChangeGraphic();
            }
            else if (this.compGlower != null && this.compGlower.Glows != shouldBeLitNow)
            {
                if (!shouldBeLitNow)
                {
                    this.RemoveGlower(this.parent.Map);
                }
                else
                {
                    this.UpdateGlower(currentColorInd);
                    this.ChangeGraphic();
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.currentColor = Props.colorOptions[currentColorInd];
            this.dirty = true;
        }

        public override void PostDeSpawn(Map map)
        {
            this.RemoveGlower(map);
            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            this.RemoveGlower(previousMap);
            base.PostDestroy(mode, previousMap);
        }
        public override void CompTick()
        {
            base.CompTick();
            if (dirty)
            {
                if (ShouldBeLitNow)
                {
                    this.UpdateGlower(currentColorInd);
                    this.ChangeGraphic();
                }
                else
                {
                    RemoveGlower(this.parent.Map);
                }
                dirty = false;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (base.parent.Faction == Faction.OfPlayer && this.Props.colorOptions.Count > 1)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.disabled = !ShouldBeLitNow;
                command_Action.disabledReason = "VFE.ColorSwitchPowerOff".Translate();
                command_Action.action = delegate
                {
                    SwitchColor();
                };
                command_Action.defaultLabel = "VFE.SwitchLightColor".Translate();
                command_Action.defaultDesc = "VFE.SwitchLightColorDesc".Translate();
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Gizmo/LampColourSwitch");
                yield return command_Action;
            }
        }

        private void SwitchColor()
        {
            if (this.currentColorInd == Props.colorOptions.Count - 1)
            {
                this.UpdateGlower(0);
                this.ChangeGraphic();
            }
            else
            {
                this.UpdateGlower(this.currentColorInd + 1);
                this.ChangeGraphic();
            }
        }
        public void RemoveGlower(Map map)
        {
            if (this.compGlower != null)
            {
                map.glowGrid?.DeRegisterGlower(this.compGlower);
                this.compGlower = null;
            }
        }

        private static ThingDef dummyDef;
        public static ThingDef GetDummyDef()
        {
            if (dummyDef is null)
            {
                dummyDef = new ThingDef
                {
                    defName = "WallLightDummyWorkaround",
                    thingClass = typeof(ThingWithComps),
                    altitudeLayer = AltitudeLayer.FloorEmplacement,
                    rotatable = false,
                    passability = Traversability.Standable,
                };
            }
            return dummyDef;
        }

        public void UpdateGlower(int colorOptionInd)
        {
            RemoveGlower(this.parent.Map);
            var colorOption = Props.colorOptions[colorOptionInd];
            this.currentColor = colorOption;
            this.currentColorInd = colorOptionInd;
            this.compGlower = new CompGlower();
            Thing dummyThing = null;
            if (Props.spawnGlowerInFacedCell)
            {
                dummyThing = ThingMaker.MakeThing(GetDummyDef());
                var cellGlower = this.parent.Position + base.parent.Rotation.FacingCell;
                GenSpawn.Spawn(dummyThing, cellGlower, this.parent.Map);
                this.compGlower.parent = dummyThing as ThingWithComps;
            }
            else
            {
                this.compGlower.parent = this.parent;
            }
            this.compGlower.Initialize(new CompProperties_Glower()
            {
                glowColor = colorOption.glowColor,
                glowRadius = colorOption.glowRadius,
                overlightRadius = colorOption.overlightRadius
            });
            Traverse.Create(this.compGlower).Field("glowOnInt").SetValue(true);
            base.parent.Map.mapDrawer.MapMeshDirty(base.parent.Position, MapMeshFlag.Things);
            base.parent.Map.glowGrid.RegisterGlower(this.compGlower);
            if (Props.spawnGlowerInFacedCell)
            {
                dummyThing.DeSpawn();
            }
        }


        public void ChangeGraphic()
        {
            if (!this.currentColor.texPath.NullOrEmpty())
            {
                var graphicData = new GraphicData();
                graphicData.graphicClass = this.parent.def.graphicData.graphicClass;
                graphicData.texPath = this.currentColor.texPath;
                graphicData.shaderType = this.parent.def.graphicData.shaderType;
                graphicData.drawSize = this.parent.def.graphicData.drawSize;
                graphicData.color = this.parent.def.graphicData.color;
                graphicData.colorTwo = this.parent.def.graphicData.colorTwo;

                var newGraphic = graphicData.GraphicColoredFor(this.parent);
                Traverse.Create(this.parent).Field("graphicInt").SetValue(newGraphic);
                base.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "PowerTurnedOn":
                case "PowerTurnedOff":
                case "FlickedOn":
                case "FlickedOff":
                case "Refueled":
                case "RanOutOfFuel":
                case "ScheduledOn":
                case "ScheduledOff":
                case "MechClusterDefeated":
                case "Hackend":
                case "RitualTargetChanged":
                case "CrateContentsChanged":
                    UpdateLit();
                    break;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentColorInd, "currentColorInd");
            this.currentColor = Props.colorOptions[currentColorInd];
        }
    }
}
