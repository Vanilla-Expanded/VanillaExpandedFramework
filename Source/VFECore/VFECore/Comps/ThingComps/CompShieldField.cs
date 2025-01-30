using AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public class Command_ActionWithCooldown : Command_Action
    {
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.6f));
        private int lastUsedTick;
        private int cooldownTicks;
        public Command_ActionWithCooldown(int lastUsedTick, int cooldownTicks)
        {
            this.lastUsedTick = lastUsedTick;
            this.cooldownTicks = cooldownTicks;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            if (this.lastUsedTick > 0)
            {
                var cooldownTicksRemaining = Find.TickManager.TicksGame - this.lastUsedTick;
                if (cooldownTicksRemaining < this.cooldownTicks)
                {
                    float num = Mathf.InverseLerp(this.cooldownTicks, 0, cooldownTicksRemaining);
                    Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
                }
            }
            if (result.State == GizmoState.Interacted)
            {
                return result;
            }
            return new GizmoResult(result.State);
        }
    }
    public class CompProperties_ShieldField : CompProperties
    {
        public bool activeAlways;
        public float initialEnergyPercentage;
        public int rechargeTicksWhenDepleted;
        public float shortCircuitChancePerEnergyLost;
        public float inactivePowerConsumption;
        public Color shieldColour = Color.white;
        public StatDef rechargeRateStat;
        public StatDef shieldEnergyMaxStat;
        public StatDef shieldRadiusStat;
        public int workingTimeTicks = -1;
        public int cooldownTicks = -1;
        public bool manualActivation;
        public string activationLabelKey;
        public string activationDescKey;
        public string activationIconTexPath;
        public int disarmedByEmpForTicks = -1;
        public SoundDef activeSound;
        public bool toggleable = false;
        public string toggleIconPath = "UI/ToggleIcon";
        public string toggleLabelKey;
        public string toggleDescKey;
        public EffecterDef reactivateEffect;
        public CompProperties_ShieldField()
        {
            this.compClass = typeof(CompShieldField);
        }
    }

    [StaticConstructorOnStartup]
    public class CompShieldField : ThingComp, PawnGizmoProvider
    {
        public bool initialized;
        public bool active;
        public Dictionary<Thing, int> affectedThings = new Dictionary<Thing, int>();
        public HashSet<IntVec3> coveredCells;
        public HashSet<IntVec3> scanCells;
        private const int CacheUpdateInterval = 15;
        private const float EdgeCellRadius = 5;
        private const float EnergyLossPerDamage = 0.033f;
        private int lastTimeActivated;
        private int lastTimeDisabled;
        private int lastTimeDisarmed;
        private static readonly Material BaseBubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.MoteGlow);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();
        private List<Thing> affectedThingsKeysWorkingList;
        private List<int> affectedThingsValuesWorkingList;
        private CompPowerTrader cachedPowerComp;
        private bool checkedPowerComp;
        private float energy;
        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick;
        private int shieldBuffer = 0;
        private int ticksToRecharge;
        private bool toggleIsActive = true;

        public Thing HostThing
        {
            get
            {
                if (this.parent is Apparel apparel && apparel.Wearer != null)
                {
                    return apparel.Wearer;
                }
                return this.parent;
            }
        }

        public Faction HostFaction => HostThing.Faction;

        public bool Indestructible => Props.shieldEnergyMaxStat is null;

        public static Dictionary<Map, List<CompShieldField>> listerShieldGensByMaps = new Dictionary<Map, List<CompShieldField>>();
        public static IEnumerable<CompShieldField> ListerShieldGensActiveIn(Map map)
        {
            if (listerShieldGensByMaps.TryGetValue(map, out var list))
            {
                foreach (var shield in list.Where(g => g.active && (g.Energy > 0 || g.Indestructible)))
                {
                    yield return shield;
                }
            }
        }
        public float Energy
        {
            get => energy;
            set
            {
                energy = Mathf.Clamp(value, 0, MaxEnergy);
                if (energy == 0)
                    Notify_EnergyDepleted();
            }
        }
        public CompProperties_ShieldField Props => base.props as CompProperties_ShieldField;

        private float _cachedEnergyGainPerTick, _cachedMaxEnergy, _cachedShieldRadius;
        private bool _isCacheValid = false;

        public float EnergyGainPerTick
        {
            get
            {
                if (!_isCacheValid) UpdateStatsCache();
                return _cachedEnergyGainPerTick;
            }
        }

        public float MaxEnergy
        {
            get
            {
                if (!_isCacheValid) UpdateStatsCache();
                return _cachedMaxEnergy;
            }
        }

        public float ShieldRadius
        {
            get
            {
                if (!_isCacheValid) UpdateStatsCache();
                return _cachedShieldRadius;
            }
        }

        private void UpdateStatsCache()
        {
            if (Props.rechargeRateStat != null)
            {
                _cachedEnergyGainPerTick = this.parent.GetStatValue(Props.rechargeRateStat) / 60;
            }
            if (Props.shieldEnergyMaxStat != null)
            {
                _cachedMaxEnergy = this.parent.GetStatValue(Props.shieldEnergyMaxStat);
            }
            _cachedShieldRadius = this.parent.GetStatValue(Props.shieldRadiusStat);
            _isCacheValid = true;
        }

        public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;
        public float TargetPriorityFactor => 1;

        public (HashSet<Thing> thingsWithinRadius, List<Thing> projectilesWithinScanArea) GetThingsInAreas()
        {
            var thingsWithinRadius = new HashSet<Thing>();
            var projectilesWithinScanArea = new List<Thing>();
            var map = HostThing?.MapHeld;

            if (map == null) return (thingsWithinRadius, projectilesWithinScanArea);

            if (scanCells != null && coveredCells != null)
            {
                var hostCenter = HostThing.TrueCenter().Yto0();
                var shieldRadius = ShieldRadius;
                var projectiles = map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile);
                foreach (var projectile in projectiles)
                {
                    if (scanCells.Contains(projectile.Position))
                    {
                        var distance = Vector3.Distance(hostCenter, projectile.TrueCenter().Yto0());
                        if (distance <= shieldRadius)
                        {
                            projectilesWithinScanArea.Add(projectile);
                        }
                    }
                }

                if (projectilesWithinScanArea.Any())
                {
                    foreach (var cell in coveredCells)
                    {
                        var thingList = map.thingGrid.ThingsListAtFast(cell);
                        thingsWithinRadius.AddRange(thingList);
                    }
                }
            }
            return (thingsWithinRadius, projectilesWithinScanArea);
        }

        public IEnumerable<Thing> ThingsWithinRadius
        {
            get
            {
                var map = HostThing?.MapHeld;
                if (map != null && coveredCells != null)
                {
                    foreach (var cell in coveredCells)
                    {
                        var thingList = map.thingGrid.ThingsListAtFast(cell);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            yield return thingList[i];
                        }
                    }
                }
            }
        }

        public IEnumerable<Thing> ThingsWithinScanArea
        {
            get
            {
                var map = HostThing?.MapHeld;
                if (map != null && scanCells != null)
                {
                    var hostCenter = this.HostThing.TrueCenter().Yto0();
                    var shieldRadius = ShieldRadius;
                    foreach (var cell in scanCells)
                    {
                        var thingList = map.thingGrid.ThingsListAtFast(cell);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            var thing = thingList[i];
                            var distance = Vector3.Distance(hostCenter, thing.TrueCenter().Yto0());
                            if (distance <= shieldRadius)
                                yield return thing;
                        }
                    }
                }
            }
        }

        private bool CanFunction => (!Props.toggleable || toggleIsActive) && (PowerTraderComp == null
            || PowerTraderComp.PowerOn) && !parent.IsBrokenDown() && HostThing is not Apparel;
        
        private CompPowerTrader PowerTraderComp
        {
            get
            {
                if (!checkedPowerComp)
                {
                    cachedPowerComp = parent.GetComp<CompPowerTrader>();
                    checkedPowerComp = true;
                }
                return cachedPowerComp;
            }
        }
        public void AbsorbDamage(float amount, DamageDef def, Thing source)
        {
            AbsorbDamage(amount, def, (HostThing.TrueCenter() - source.TrueCenter()).AngleFlat());
        }

        public void AbsorbDamage(float amount, DamageDef def, float angle)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(HostThing.Position, HostThing.Map, false));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(angle);
            Vector3 loc = HostThing.TrueCenter() + impactAngleVect.RotatedBy(180f) * (ShieldRadius / 2);
            float flashSize = Mathf.Min(10f, 2f + amount / 10f);
            FleckMaker.Static(HostThing.TrueCenter(), HostThing.Map, FleckDefOf.ExplosionFlash, 12);
            int dustCount = (int)flashSize;
            for (int i = 0; i < dustCount; i++)
            {
                FleckMaker.ThrowDustPuff(loc, HostThing.Map, Rand.Range(0.8f, 1.2f));
            }
            if (!Indestructible)
            {
                float energyLoss = amount * EnergyLossMultiplier(def) * EnergyLossPerDamage;
                Energy -= energyLoss;
                // try to do short circuit
                if (Rand.Chance(energyLoss * Props.shortCircuitChancePerEnergyLost))
                    GenExplosion.DoExplosion(HostThing.OccupiedRect().RandomCell, HostThing.Map, 1.9f, DamageDefOf.Flame, null);
            }
            if (Props.disarmedByEmpForTicks != -1 && def == DamageDefOf.EMP)
            {
                BreakShield(new DamageInfo(def, amount, 0, angle));
            }
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
        }

        public override void PostDeSpawn(Map map)
        {
            coveredCells = null;
            scanCells = null;
            if (listerShieldGensByMaps.TryGetValue(map, out var list))
            {
                list.Remove(this);
            }
            base.PostDeSpawn(map);
        }

        public bool IsApparel => parent is Apparel;
        private bool IsBuiltIn => !IsApparel;

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (IsApparel)
            {
                Draw();
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (IsBuiltIn)
            {
                Draw();
            }
        }

        private void Draw()
        {
            if (active && (Energy > 0 || Indestructible))
            {
                float size = ShieldRadius * 2;
                if (!Indestructible)
                {
                    size *= Mathf.Lerp(0.9f, 1.1f, Energy / MaxEnergy);
                }
                Vector3 pos = HostThing.TrueCenter();
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

                int ticksSinceAbsorbDamage = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (ticksSinceAbsorbDamage < 8)
                {
                    float sizeMod = (8 - ticksSinceAbsorbDamage) / 8f * 0.05f;
                    pos += impactAngleVect * sizeMod;
                    size -= sizeMod;
                }

                float angle = Rand.Range(0, 45);
                Vector3 s = new Vector3(size, 1f, size);
                Matrix4x4 matrix = default;
                matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);

                MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, Props.shieldColour);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BaseBubbleMat, 0, null, 0, MatPropertyBlock);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref affectedThings, "affectedThings", LookMode.Reference, LookMode.Value, ref affectedThingsKeysWorkingList, ref affectedThingsValuesWorkingList);
            Scribe_Values.Look(ref ticksToRecharge, "ticksToRecharge");
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref shieldBuffer, "shieldBuffer");
            Scribe_Values.Look(ref active, "active");
            Scribe_Values.Look(ref lastTimeActivated, "lastTimeActivated");
            Scribe_Values.Look(ref lastTimeDisabled, "lastTimeDisabled");
            Scribe_Values.Look(ref lastTimeDisarmed, "lastTimeDisarmed");
            Scribe_Values.Look(ref toggleIsActive, "toggleIsActive", true);
            Scribe_Values.Look(ref initialized, "initialized");
        }

        public bool CanActivateShield()
        {
            if (Props.manualActivation)
            {
                if (this.lastTimeDisabled > 0 && Find.TickManager.TicksGame - this.lastTimeDisabled < this.Props.cooldownTicks)
                {
                    return false;
                }
            }
            if (Props.disarmedByEmpForTicks != -1 && this.lastTimeDisarmed > 0 && Find.TickManager.TicksGame - this.lastTimeDisarmed < Props.disarmedByEmpForTicks)
            {
                return false;
            }
            return true;
        }

        public bool ManuallyActivated => this.lastTimeActivated > 0 && Find.TickManager.TicksGame - this.lastTimeActivated < Props.workingTimeTicks;

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            if (this.HostThing.Faction == Faction.OfPlayer)
            {
                // Shield health
                if (!Indestructible && Find.Selector.SingleSelectedThing == this.HostThing)
                {
                    yield return new Gizmo_EnergyShieldGeneratorStatus()
                    {
                        shieldGenerator = this
                    };
                }
                if (Props.manualActivation)
                {
                    yield return new Command_ActionWithCooldown(this.lastTimeDisabled, this.Props.cooldownTicks)
                    {
                        defaultLabel = Props.activationLabelKey.Translate(),
                        defaultDesc = Props.activationDescKey.Translate(),
                        icon = ContentFinder<Texture2D>.Get(Props.activationIconTexPath),
                        action = delegate
                        {
                            this.lastTimeActivated = Find.TickManager.TicksGame;
                            this.lastTimeDisabled = 0;
                            this.active = true;
                        },
                        Disabled = ManuallyActivated || !CanActivateShield()
                    };
                }
            }


            foreach (var gizmo in base.CompGetWornGizmosExtra())
                yield return gizmo;
        }


        public override string CompInspectStringExtra()
        {
            var inspectBuilder = new StringBuilder();

            // Inactive
            if (!active)
                inspectBuilder.AppendLine("InactiveFacility".Translate().CapitalizeFirst());

            inspectBuilder.AppendLine(base.CompInspectStringExtra());

            return inspectBuilder.ToString().TrimEndNewlines();
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {            // EMP - direct
            if (dinfo.Def == DamageDefOf.EMP && !Indestructible)
            {
                if (Props.disarmedByEmpForTicks != -1)
                {
                    BreakShield(dinfo);
                }
                else
                {
                    Energy = 0;
                }
            }
            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }

        [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
        class SpawnSetup_Patch
        {
            public static void Postfix(Pawn __instance)
            {
                if (__instance.apparel?.WornApparel != null)
                {
                    foreach (var apparel in __instance.apparel.WornApparel)
                    {
                        var comp = apparel.GetComp<CompShieldField>();
                        if (comp != null)
                        {
                            comp.Initialize();
                        }
                    }
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Initialize();
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            Initialize();
        }

        public void Initialize()
        {
            if (this.parent != null)
            {
                if (this.Props is null)
                {
                    var modName = this.parent.def.modContentPack?.Name;
                    var errorMessage = this.parent.def + " has " + this.GetType() + " but lacks CompProperties_ShieldField properties. It must be set in XML in order to work.";
                    if (modName.NullOrEmpty() is false)
                    {
                        errorMessage += " Report about it to " + modName + " author";
                    }
                    Log.Error(errorMessage);
                }
                if (HostThing.Map != null)
                {
                    if (!listerShieldGensByMaps.TryGetValue(HostThing.Map, out var list))
                    {
                        listerShieldGensByMaps[HostThing.Map] = list = new List<CompShieldField>();
                    }
                    if (!list.Contains(this))
                    {
                        list.Add(this);
                    }

                    UpdateShieldCoverage();
                }

                if (initialized is false)
                {
                    if (Indestructible is false)
                    {
                        energy = MaxEnergy * Props.initialEnergyPercentage;
                    }
                    initialized = true;
                }
            }
        }

        private void UpdateShieldCoverage()
        {
            if (HostThing.Map != null)
            {
                // Set up shield coverage
                coveredCells = new HashSet<IntVec3>(GenRadial.RadialCellsAround(HostThing.Position, ShieldRadius, true).Where(x => x.InBounds(HostThing.Map)));
                if (ShieldRadius < EdgeCellRadius + 1)
                    scanCells = coveredCells;
                else
                {
                    IEnumerable<IntVec3> interiorCells = GenRadial.RadialCellsAround(HostThing.Position, ShieldRadius - EdgeCellRadius, true);
                    scanCells = new HashSet<IntVec3>(coveredCells.Where(c => !interiorCells.Contains(c)));
                }
            }
            
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            // No energy
            if (!Indestructible && Energy == 0)
                return true;

            // Attacker isn't using EMPs
            if (!disabledFor.CurrentEffectiveVerb.IsEMP())
                return true;

            // Return whether or not the shield can function
            return !CanFunction;
        }

        public bool ReactivatedThisTick => Find.TickManager.TicksGame - lastAbsorbDamageTick == Props.cooldownTicks;

        private Sustainer sustainer;
        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(CacheUpdateInterval))
                UpdateCache();

            if (Props.workingTimeTicks != -1 && this.lastTimeActivated > 0 && Find.TickManager.TicksGame - this.lastTimeActivated >= this.Props.workingTimeTicks)
            {
                this.lastTimeDisabled = Find.TickManager.TicksGame;
                this.lastTimeActivated = 0;
            }

            if (ReactivatedThisTick && Props.reactivateEffect != null)
            {
                Effecter effecter = new Effecter(Props.reactivateEffect);
                effecter.Trigger(parent, TargetInfo.Invalid);
                effecter.Cleanup();
            }
            if (CanFunction)
            {
                if (!Indestructible)
                {
                    // Recharge shield
                    if (ticksToRecharge > 0)
                    {
                        ticksToRecharge--;
                        if (ticksToRecharge == 0)
                        {
                            if (Props.rechargeRateStat is null)
                            {
                                Energy = MaxEnergy;
                            }
                        }
                    }
                    else
                    {
                        Energy += EnergyGainPerTick;
                    }
                }

                // If shield is active
                if (active)
                {
                    if (Props.activeSound != null)
                    {
                        if (sustainer == null || sustainer.Ended)
                        {
                            sustainer = Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(HostThing));
                        }
                        sustainer?.Maintain();
                    }

                    // Power consumption
                    if (PowerTraderComp != null)
                        PowerTraderComp.PowerOutput = -PowerTraderComp.Props.PowerConsumption;

                    if (Energy > 0 || Indestructible)
                        EnergyShieldTick();
                }
                else if (PowerTraderComp != null)
                    PowerTraderComp.PowerOutput = -Props.inactivePowerConsumption;

                if (Props.activeSound != null && !active && sustainer != null && !sustainer.Ended)
                {
                    sustainer.End();
                }
            }
            else if (PowerTraderComp != null)
                PowerTraderComp.PowerOutput = -Props.inactivePowerConsumption;

            base.CompTick();
        }

        private void BreakShield(DamageInfo dinfo)
        {
            float fTheta;
            Vector3 center;
            if (active)
            {
                VFEDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(HostThing));
                int num = Mathf.CeilToInt(ShieldRadius * 2f);
                fTheta = (float)Math.PI * 2f / (float)num;
                center = HostThing.TrueCenter();
                for (int i = 0; i < num; i++)
                {
                    FleckMaker.ConnectingLine(PosAtIndex(i), PosAtIndex((i + 1) % num), FleckDefOf.LineEMP, HostThing.Map, 1.5f);
                }
            }

            if (HostThing is Pawn pawn)
            {
                dinfo.SetAmount((float)Props.disarmedByEmpForTicks / 30f);
                pawn.stances.stunner.Notify_DamageApplied(dinfo);
            }
            if (!Indestructible)
            {
                Energy = 0;
            }
            lastTimeDisarmed = Find.TickManager.TicksGame;
            Vector3 PosAtIndex(int index)
            {
                return new Vector3(ShieldRadius * Mathf.Cos(fTheta * (float)index) + center.x, 0f, ShieldRadius * Mathf.Sin(fTheta * (float)index) + center.z);
            }
        }

        public bool WithinBoundary(IntVec3 sourcePos, IntVec3 checkedPos)
        {
            return (coveredCells.Contains(sourcePos) && coveredCells.Contains(checkedPos)) || (!coveredCells.Contains(sourcePos) && !coveredCells.Contains(checkedPos));
        }

        private float EnergyLossMultiplier(DamageDef damageDef)
        {
            // EMP - on shield
            if (damageDef == DamageDefOf.EMP)
                return 4;

            return 1;
        }

        private void EnergyShieldTick()
        {
            var things = GetThingsInAreas();
            foreach (var thing in things.projectilesWithinScanArea)
            {
                // Try and block projectiles from outside
                if (thing is Projectile proj && proj.BlockableByShield(this))
                {
                    if (!things.thingsWithinRadius.Contains(proj.Launcher))
                    {
                        // Explosives are handled separately
                        if (!(proj is Projectile_Explosive) || proj.def.projectile.damageDef == DamageDefOf.EMP)
                            AbsorbDamage(proj.DamageAmount, proj.def.projectile.damageDef, proj.ExactRotation.eulerAngles.y);
                        proj.Position += Rot4.FromAngleFlat((HostThing.Position - proj.Position).AngleFlat).Opposite.FacingCell;
                        NonPublicFields.Projectile_usedTarget.SetValue(proj, new LocalTargetInfo(proj.Position));
                        NonPublicMethods.Projectile_ImpactSomething(proj);
                    }
                }
            }
        }
        private void Notify_EnergyDepleted()
        {
            VFEDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(HostThing.Position, HostThing.Map));
            FleckMaker.Static(HostThing.TrueCenter(), HostThing.Map, FleckDefOf.ExplosionFlash, 12);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = HostThing.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                FleckMaker.ThrowDustPuff(loc, HostThing.Map, Rand.Range(0.8f, 1.2f));
            }
            ticksToRecharge = Props.rechargeTicksWhenDepleted;
        }

        private void UpdateCache()
        {
            UpdateShieldCoverage();
            if (affectedThings is null)
            {
                affectedThings = new Dictionary<Thing, int>();
            }
            for (int i = 0; i < affectedThings.Count; i++)
            {
                var curKey = affectedThings.Keys.ToList()[i];
                if (affectedThings[curKey] <= 0)
                    affectedThings.Remove(curKey);
                else
                    affectedThings[curKey] -= CacheUpdateInterval;
            }

            if (!Props.manualActivation)
            {
                active = this.HostThing.Map != null && CanFunction &&
                (Props.activeAlways ||
                HostFaction != null && GenHostility.AnyHostileActiveThreatTo(HostThing.Map, HostFaction) ||
                HostThing.Map.listerThings.ThingsOfDef(VFEDefOf.Tornado).Any() ||
                HostThing.Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.DropPodIncoming).Any() || shieldBuffer > 0);
            }
            if (active)
            {
                active = CanActivateShield();
            }
            if (HostThing.Map != null)
            {
                if ((HostFaction != null && GenHostility.AnyHostileActiveThreatTo(HostThing.Map, HostFaction)
                    || HostThing.Map.listerThings.ThingsOfDef(VFEDefOf.Tornado).Any()
                    || HostThing.Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.DropPodIncoming).Any()) && shieldBuffer < 15)
                    shieldBuffer = 15;
                else
                    shieldBuffer -= 1;
            }
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (this.HostThing.Faction == Faction.OfPlayer)
            {
                // Shield health
                if (!Indestructible && Find.Selector.SingleSelectedThing == this.parent)
                {
                    yield return new Gizmo_EnergyShieldGeneratorStatus()
                    {
                        shieldGenerator = this
                    };
                }

                if (Props.toggleable)
                {
                    yield return new Command_Toggle
                    {
                        defaultLabel = Props.toggleLabelKey.Translate(toggleIsActive ? "On".Translate() : "Off".Translate()),
                        defaultDesc = Props.toggleDescKey.Translate(),
                        icon = ContentFinder<Texture2D>.Get(Props.toggleIconPath),
                        toggleAction = delegate 
                        { 
                            toggleIsActive = !toggleIsActive; 
                            UpdateCache();
                        },
                        isActive = () => toggleIsActive,
                    };
                }

                if (Props.manualActivation)
                {
                    yield return new Command_ActionWithCooldown(this.lastTimeDisabled, this.Props.cooldownTicks)
                    {
                        defaultLabel = Props.activationLabelKey.Translate(),
                        defaultDesc = Props.activationDescKey.Translate(),
                        icon = ContentFinder<Texture2D>.Get(Props.activationIconTexPath),
                        action = delegate
                        {
                            this.lastTimeActivated = Find.TickManager.TicksGame;
                            this.lastTimeDisabled = 0;
                            this.active = true;
                        },
                        Disabled = ManuallyActivated || !CanActivateShield()
                    };
                }
            }
        }
    }

    public static class ShieldGeneratorUtility
    {
        public static bool AffectsShields(this DamageDef damageDef)
        {
            return damageDef.isExplosive || damageDef == DamageDefOf.EMP;
        }
        public static void CheckIntercept(Thing thing, Map map, int damageAmount, DamageDef damageDef, Func<IEnumerable<IntVec3>> cellGetter, Func<bool> canIntercept = null, Func<CompShieldField, bool> preIntercept = null, Action<CompShieldField> postIntercept = null)
        {
            if (canIntercept == null || canIntercept())
            {
                var occupiedCells = new HashSet<IntVec3>(cellGetter());
                var listerShields = CompShieldField.ListerShieldGensActiveIn(map).ToList();
                for (int i = 0; i < listerShields.Count; i++)
                {
                    var shield = listerShields[i];
                    var coveredCells = new HashSet<IntVec3>(shield.coveredCells);
                    if ((preIntercept == null || preIntercept.Invoke(shield)) && occupiedCells.Any(c => coveredCells.Contains(c)))
                    {
                        shield.AbsorbDamage(damageAmount, damageDef, thing);
                        postIntercept?.Invoke(shield);
                        return;
                    }
                }
            }
        }

        public static bool BlockableByShield(this Projectile proj, CompShieldField shieldGen)
        {
            if (!proj.def.projectile.flyOverhead)
                return true;
            return !shieldGen.coveredCells.Contains(((Vector3)NonPublicFields.Projectile_origin.GetValue(proj)).ToIntVec3()) &&
                (int)NonPublicFields.Projectile_ticksToImpact.GetValue(proj) / (float)NonPublicProperties.Projectile_get_StartingTicksToImpact(proj) <= 0.5f;
        }

        //---added--- inspired by Frontier Security's method (distributed under an open-source non-profit license)
        public static bool CheckPodHostility(CompShieldField shield, DropPodIncoming dropPod)  //this is me. Didn't want to zap trader deliveries or allies
        {
            var innerContainer = dropPod.Contents.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (innerContainer[i] is Pawn pawn)
                {
                    if (shield.HostFaction != null && pawn.HostileTo(shield.HostFaction))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void KillPawn(Pawn pawn, IntVec3 position, Map map) //FD inspired - means zapped pawns actually count as dead. FD has a tale recorder in here so cause of death is right, but should work ok.
        {
            // spawn on map for just an instant
            GenPlace.TryPlaceThing(pawn, position, map, ThingPlaceMode.Near);
            pawn.inventory.DestroyAll();
            pawn.Kill(new DamageInfo(DamageDefOf.Crush, 100));
            pawn.Corpse.Destroy();
        }

    }

    [StaticConstructorOnStartup]
    public class Gizmo_EnergyShieldGeneratorStatus : Gizmo
    {
        public Gizmo_EnergyShieldGeneratorStatus()
        {
            Order = -100;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, shieldGenerator.parent.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float displayEnergy = shieldGenerator.Energy;
                float fillPercent = displayEnergy / shieldGenerator.MaxEnergy;
                Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (displayEnergy * 100).ToString("F0") + " / " + (shieldGenerator.MaxEnergy * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        public CompShieldField shieldGenerator;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }

    [HarmonyPatch(typeof(Tornado), "CellImmuneToDamage")]
    public static class CellImmuneToDamage
    {
        public static void Postfix(Tornado __instance, IntVec3 c, ref bool __result)
        {
            // Shield-covered cells are immune to damage
            if (!__result)
            {
                List<CompShieldField> shieldGens = CompShieldField.ListerShieldGensActiveIn(__instance.Map).ToList();
                for (int i = 0; i < shieldGens.Count; i++)
                {
                    CompShieldField gen = shieldGens[i];
                    if (gen.coveredCells.Contains(c))
                    {
                        if (gen.affectedThings is null)
                        {
                            gen.affectedThings = new Dictionary<Thing, int>();
                        }
                        if (!gen.affectedThings.ContainsKey(__instance))
                        {
                            gen.AbsorbDamage(30, DamageDefOf.TornadoScratch, __instance);
                            gen.affectedThings.Add(__instance, 15);
                        }
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Skyfaller), nameof(Skyfaller.Tick))]
    public static class Patch_Tick
    {
        public static void Prefix(Skyfaller __instance) //patch the tick, not the creation - means shields can turn on in time to do something
        {
            if (__instance.Map != null && __instance.ticksToImpact <= 20)
            {
                var thingDefExtension = __instance.def.GetModExtension<ThingDefExtension>();
                if (thingDefExtension != null)
                {
                    ShieldGeneratorUtility.CheckIntercept(__instance, __instance.Map, thingDefExtension.shieldDamageIntercepted, DamageDefOf.Blunt,
                            () => __instance.OccupiedRect().Cells,
                            () => thingDefExtension.shieldDamageIntercepted > -1,
                        preIntercept: (CompShieldField x) => __instance is not DropPodIncoming dropPodIncoming
                            || ShieldGeneratorUtility.CheckPodHostility(x, dropPodIncoming),
                        postIntercept: s =>
                        {
                            if (s.Energy > 0)
                            {
                                switch (__instance)
                                {
                                    case DropPodIncoming dropPod:
                                        var innerContainer = dropPod.Contents.innerContainer;
                                        for (int i = 0; i < innerContainer.Count; i++)
                                        {
                                            var thing = innerContainer[i];
                                            if (thing is Pawn pawn)
                                                ShieldGeneratorUtility.KillPawn(pawn, dropPod.Position, dropPod.Map);
                                        }
                                        dropPod.Destroy();
                                        return;
                                    case FlyShipLeaving _:
                                        return;
                                    default:
                                        {
                                            __instance.Destroy();
                                            return;
                                        }
                                }
                            }
                        });
                }

            }
        }
    }
}