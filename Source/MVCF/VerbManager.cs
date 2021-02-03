using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Harmony;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MVCF
{
    public class VerbManager : IVerbOwner
    {
        private readonly List<ManagedVerb> drawVerbs = new List<ManagedVerb>();
        public readonly List<TurretVerb> tickVerbs = new List<TurretVerb>();
        private readonly List<ManagedVerb> verbs = new List<ManagedVerb>();
        public Verb CurrentVerb;
        public DebugOptions debugOpts;
        public bool HasVerbs;
        public Verb OverrideVerb;
        public Verb SearchVerb;
        public bool NeedsTicking { get; private set; }

        public IEnumerable<Verb> AllVerbs => verbs.Select(mv => mv.Verb);
        public IEnumerable<Verb> AllRangedVerbs => verbs.Select(mv => mv.Verb).Where(verb => !verb.IsMeleeAttack);

        public IEnumerable<Verb> AllRangedVerbsNoEquipment =>
            verbs.Where(mv => mv.Source != VerbSource.Equipment).Select(mv => mv.Verb);

        public IEnumerable<ManagedVerb> ManagedVerbs => verbs;

        public IEnumerable<Verb> AllRangedVerbsNoEquipmentNoApparel => verbs
            .Where(mv => mv.Source != VerbSource.Equipment && mv.Source != VerbSource.Apparel).Select(mv => mv.Verb);

        public Pawn Pawn { get; private set; }

        public string UniqueVerbOwnerID()
        {
            return "VerbManager_" + (Pawn as IVerbOwner).UniqueVerbOwnerID();
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return p == Pawn;
        }

        public VerbTracker VerbTracker { get; private set; }

        public List<VerbProperties> VerbProperties => new List<VerbProperties>
        {
            new VerbProperties
            {
                range = 0,
                minRange = 9999,
                targetParams = new TargetingParameters(),
                verbClass = typeof(Verb_Search),
                label = Base.SearchLabel,
                defaultProjectile = ThingDef.Named("Bullet_Revolver"),
                onlyManualCast = false
            }
        };

        public List<Tool> Tools => new List<Tool>();
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;
        public Thing ConstantCaster => Pawn;

        public ManagedVerb GetManagedVerbForVerb(Verb verb, bool warnOnFailed = true)
        {
            var mv = verbs.FirstOrFallback(v => v.Verb == verb);
            if (mv == null && warnOnFailed)
                Log.ErrorOnce("[MVCF] Attempted to get ManagedVerb for verb " + verb.Label() +
                              " which does not have one. This may cause issues.", verb.Label().GetHashCode());

            return mv;
        }

        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            VerbTracker = new VerbTracker(this);
            SearchVerb = (Verb_Search) VerbTracker.PrimaryVerb;
            NeedsTicking = false;
            debugOpts.ScoreLogging = false;
            debugOpts.VerbLogging = false;
            if (!Base.Features.RangedAnimals && pawn.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack))
            {
                Log.Error(
                    "[MVCF] Found pawn with native ranged verbs while that feature is not enabled. Enabling now. This is not recommended. Contact the author of " +
                    pawn.def.modContentPack.Name + " and ask them to add a MVCF.ModDef.");
                Base.Features.RangedAnimals = true;
                Base.ApplyPatches();
            }

            foreach (var verb in pawn.VerbTracker.AllVerbs)
                AddVerb(verb, VerbSource.RaceDef, pawn.TryGetComp<Comp_VerbGiver>()?.PropsFor(verb));
            if (pawn?.health?.hediffSet?.hediffs != null)
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
                    if (comp != null)
                    {
                        var extComp = comp as HediffComp_ExtendedVerbGiver;
                        foreach (var verb in comp.VerbTracker.AllVerbs)
                            AddVerb(verb, VerbSource.Hediff, extComp?.PropsFor(verb));
                    }
                }

            if (pawn?.apparel?.WornApparel != null)
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    var comp = apparel.TryGetComp<Comp_VerbGiver>();
                    if (comp == null) continue;
                    foreach (var verb in comp.VerbTracker.AllVerbs)
                        AddVerb(verb, VerbSource.Apparel, comp.PropsFor(verb));
                }

            if (pawn?.equipment?.AllEquipmentListForReading != null)
                foreach (var eq in pawn.equipment.AllEquipmentListForReading)
                {
                    var comp = eq.TryGetComp<CompEquippable>();
                    if (comp == null) continue;
                    foreach (var verb in comp.VerbTracker.AllVerbs)
                        AddVerb(verb, VerbSource.Equipment, (comp.props as CompProperties_VerbProps)?.PropsFor(verb));
                }
        }


        public void AddVerb(Verb verb, VerbSource source, AdditionalVerbProps props)
        {
            if (debugOpts.VerbLogging) Log.Message("Adding " + verb + " from " + source + " with props " + props);
            ManagedVerb mv;
            if (props != null && props.canFireIndependently)
            {
                var tv = new TurretVerb(verb, source, props, this);
                if (tickVerbs.Count == 0)
                {
                    NeedsTicking = true;
                    WorldComponent_MVCF.GetComp().TickManagers.Add(new System.WeakReference<VerbManager>(this));
                }

                tickVerbs.Add(tv);
                mv = tv;
            }
            else
            {
                mv = new ManagedVerb(verb, source, props, this);
            }

            if (props != null && props.draw) drawVerbs.Add(mv);

            verbs.Add(mv);
            RecalcSearchVerb();
        }

        public void RemoveVerb(Verb verb)
        {
            var mv = verbs.Find(m => m.Verb == verb);

            verbs.Remove(mv);
            if (drawVerbs.Contains(mv)) drawVerbs.Remove(mv);
            var idx = tickVerbs.FindIndex(tv => tv.Verb == verb);
            if (idx >= 0)
            {
                tickVerbs.RemoveAt(idx);
                if (tickVerbs.Count == 0)
                {
                    NeedsTicking = false;
                    WorldComponent_MVCF.GetComp().TickManagers.RemoveAll(wr =>
                    {
                        if (!wr.TryGetTarget(out var man)) return true;
                        return man == this;
                    });
                }
            }

            RecalcSearchVerb();
        }

        public void RecalcSearchVerb()
        {
            if (debugOpts.VerbLogging) Log.Message("RecalcSearchVerb");
            var verbsToUse = verbs
                .Where(v => v.Enabled && (v.Props == null || !v.Props.canFireIndependently) && !v.Verb.IsMeleeAttack)
                .ToList();
            if (debugOpts.VerbLogging) verbsToUse.ForEach(v => Log.Message("Verb: " + v.Verb));
            if (verbsToUse.Count == 0)
            {
                HasVerbs = false;
                if (debugOpts.VerbLogging) Log.Message("No Verbs");
                return;
            }

            HasVerbs = true;

            SearchVerb.verbProps.range = verbsToUse.Select(v => v.Verb.verbProps.range).Max();
            if (debugOpts.VerbLogging) Log.Message("Resulting range: " + SearchVerb.verbProps.range);
            SearchVerb.verbProps.minRange = verbsToUse.Select(v => v.Verb.verbProps.minRange).Min();
            if (debugOpts.VerbLogging) Log.Message("Resulting minRange: " + SearchVerb.verbProps.minRange);
            SearchVerb.verbProps.requireLineOfSight = verbsToUse.All(v => v.Verb.verbProps.requireLineOfSight);
            if (debugOpts.VerbLogging)
                Log.Message("Resulting requireLineOfSight: " + SearchVerb.verbProps.requireLineOfSight);
            SearchVerb.verbProps.mustCastOnOpenGround = verbsToUse.All(v => v.Verb.verbProps.mustCastOnOpenGround);
            if (debugOpts.VerbLogging)
                Log.Message("Resulting mustCastOnOpenGround: " + SearchVerb.verbProps.mustCastOnOpenGround);
            var targetParams = verbsToUse.Select(mv => mv.Verb.targetParams).ToList();
            SearchVerb.verbProps.targetParams = new TargetingParameters
            {
                canTargetAnimals = targetParams.Any(tp => tp.canTargetAnimals),
                canTargetBuildings = targetParams.Any(tp => tp.canTargetBuildings),
                canTargetPawns = targetParams.Any(tp => tp.canTargetPawns),
                canTargetFires = targetParams.Any(tp => tp.canTargetFires),
                canTargetHumans = targetParams.Any(tp => tp.canTargetHumans),
                canTargetItems = targetParams.Any(tp => tp.canTargetItems),
                canTargetLocations = targetParams.Any(tp => tp.canTargetLocations),
                canTargetMechs = targetParams.Any(tp => tp.canTargetMechs),
                canTargetSelf = targetParams.Any(tp => tp.canTargetSelf)
            };
        }

        public void DrawAt(Vector3 drawPos)
        {
            foreach (var mv in drawVerbs) mv.DrawOn(Pawn, drawPos);
        }

        public void Tick()
        {
            foreach (var mv in tickVerbs) mv.Tick();
        }
    }

    public class ManagedVerb
    {
        private static readonly Vector3 WestEquipOffset = new Vector3(-0.2f, 0.0367346928f, -0.22f);
        private static readonly Vector3 EastEquipOffset = new Vector3(0.2f, 0.0367346928f, -0.22f);
        private static readonly Vector3 NorthEquipOffset = new Vector3(0f, 0f, -0.11f);
        private static readonly Vector3 SouthEquipOffset = new Vector3(0f, 0.0367346928f, -0.22f);

        private static readonly Vector3 EquipPointOffset = new Vector3(0f, 0f, 0.4f);
        protected readonly VerbManager man;
        public bool Enabled = true;
        public AdditionalVerbProps Props;
        public VerbSource Source;
        public Verb Verb;

        public ManagedVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man)
        {
            Verb = verb;
            Source = source;
            Props = props;
            this.man = man;
            if (Props != null && Props.draw && !Base.Features.Drawing)
            {
                Log.Error(
                    "[MVCF] Found a verb marked to draw while that feature is not enabled. Enabling now. This is not recommend.");
                Base.Features.Drawing = true;
                Base.ApplyPatches();
            }

            if (Props != null && Props.canFireIndependently && !Base.Features.IndependentFire)
            {
                Log.Error(
                    "[MVCF] Found a verb marked to fire independently while that feature is not enabled. Enabling now. This is not recommend.");
                Base.Features.IndependentFire = true;
                Base.ApplyPatches();
            }

            if (Props != null && !Props.separateToggle && !Base.Features.IntegratedToggle)
            {
                Log.Error(
                    "[MVCF] Found a verb marked for an integrated toggle while that feature is not enabled. Enabling now. This is not recommend.");
                Base.Features.IntegratedToggle = true;
                Base.ApplyPatches();
            }
        }

        public void Toggle()
        {
            Enabled = !Enabled;
            man.RecalcSearchVerb();
        }

        public virtual void DrawOn(Pawn p, Vector3 drawPos)
        {
            if (Props == null) return;
            if (!Props.draw) return;
            if (p.Dead || !p.Spawned) return;
            drawPos.y += 0.0367346928f;
            var target = PointingTarget(p);
            DrawPointingAt(DrawPos(target, p, drawPos),
                DrawAngle(target, p, drawPos), p.BodySize);
        }

        public virtual float DrawAngle(LocalTargetInfo target, Pawn p, Vector3 drawPos)
        {
            if (target != null && target.IsValid)
            {
                var a = target.HasThing ? target.Thing.DrawPos : target.Cell.ToVector3Shifted();

                return (a - drawPos).MagnitudeHorizontalSquared() > 0.001f ? (a - drawPos).AngleFlat() : 0f;
            }

            if (Source == VerbSource.Equipment)
            {
                if (p.Rotation == Rot4.South) return 143f;

                if (p.Rotation == Rot4.North) return 143f;

                if (p.Rotation == Rot4.East) return 143f;

                if (p.Rotation == Rot4.West) return 217f;
            }

            return p.Rotation.AsAngle;
        }

        public virtual Vector3 DrawPos(LocalTargetInfo target, Pawn p, Vector3 drawPos)
        {
            if (Source == VerbSource.Equipment)
            {
                if (target != null && target.IsValid)
                    return drawPos + EquipPointOffset.RotatedBy(DrawAngle(target, p, drawPos));

                if (p.Rotation == Rot4.South) return drawPos + SouthEquipOffset;

                if (p.Rotation == Rot4.North) return drawPos + NorthEquipOffset;

                if (p.Rotation == Rot4.East) return drawPos + EastEquipOffset;

                if (p.Rotation == Rot4.West) return drawPos + WestEquipOffset;
            }

            return Props.DrawPos(p.def.defName, drawPos, p.Rotation);
        }

        public virtual LocalTargetInfo PointingTarget(Pawn p)
        {
            if (p.stances.curStance is Stance_Busy busy && !busy.neverAimWeapon && busy.focusTarg.IsValid)
                return busy.focusTarg;
            return null;
        }

        private void DrawPointingAt(Vector3 drawLoc, float aimAngle, float scale)
        {
            var num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
            }
            else
            {
                mesh = MeshPool.plane10;
            }

            num %= 360f;

            var matrix4X4 = new Matrix4x4();
            matrix4X4.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), Vector3.one * scale);

            Graphics.DrawMesh(mesh, matrix4X4, Props.Graphic.MatSingle, 0);
        }

        private static bool CarryWeaponOpenly(Pawn pawn)
        {
            if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null) return false;

            if (pawn.Drafted) return true;

            if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) return true;

            if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon) return true;

            var lord = pawn.GetLord();
            return lord?.LordJob != null && lord.LordJob.AlwaysShowWeapon;
        }
    }

    public enum VerbSource
    {
        Apparel,
        Equipment,
        Hediff,
        RaceDef
    }

    public class TurretVerb : ManagedVerb
    {
        private readonly DummyCaster dummyCaster;
        private int cooldownTicksLeft;
        private LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
        private int warmUpTicksLeft;


        public TurretVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man) : base(verb, source,
            props, man)
        {
            dummyCaster = new DummyCaster(man.Pawn, this);
            dummyCaster.Tick();
            dummyCaster.SpawnSetup(man.Pawn.Map, false);
            verb.caster = dummyCaster;
            verb.castCompleteCallback = () => cooldownTicksLeft = Verb.verbProps.AdjustedCooldownTicks(Verb, man.Pawn);
        }

        public LocalTargetInfo Target => currentTarget;

        public virtual void Tick()
        {
            if (Verb.Bursting) return;
            if (cooldownTicksLeft > 0) cooldownTicksLeft--;

            if (cooldownTicksLeft > 0) return;
            if (!Enabled)
            {
                if (currentTarget.IsValid) currentTarget = LocalTargetInfo.Invalid;
                if (warmUpTicksLeft > 0) warmUpTicksLeft = 0;
                return;
            }

            if (!currentTarget.IsValid || currentTarget.HasThing && currentTarget.ThingDestroyed ||
                currentTarget.HasThing && currentTarget.Thing is Pawn p && (p.Downed || p.Dead))
            {
                man.OverrideVerb = Verb;
                currentTarget = TryFindNewTarget();
                man.OverrideVerb = null;
                TryStartCast();
            }
            else if (warmUpTicksLeft == 0)
            {
                TryCast();
            }
            else if (warmUpTicksLeft > 0)
            {
                warmUpTicksLeft--;
            }
            else
            {
                TryStartCast();
            }
        }

        protected virtual void TryStartCast()
        {
            if (Verb.verbProps.warmupTime > 0)
                warmUpTicksLeft = (Verb.verbProps.warmupTime * man.Pawn.GetStatValue(StatDefOf.AimingDelayFactor))
                    .SecondsToTicks();
            else
                TryCast();
        }

        protected virtual void TryCast()
        {
            warmUpTicksLeft = -1;
            var success = Verb.TryStartCastOn(currentTarget);
            if (success) Verb.WarmupComplete();
        }

        public override LocalTargetInfo PointingTarget(Pawn p)
        {
            return currentTarget;
        }

        public virtual bool CanFire()
        {
            return true;
        }

        public override void DrawOn(Pawn p, Vector3 drawPos)
        {
            base.DrawOn(p, drawPos);
            if (Find.Selector.IsSelected(p) && Target.IsValid)
            {
                GenDraw.DrawAimPie(p, Target, warmUpTicksLeft, 0.2f);
                GenDraw.DrawLineBetween(drawPos, Target.HasThing ? Target.Thing.DrawPos : Target.Cell.ToVector3());
            }
        }

        public void SetTarget(LocalTargetInfo target)
        {
            currentTarget = target;
            TryStartCast();
        }

        protected virtual LocalTargetInfo TryFindNewTarget()
        {
            return AttackTargetFinder.BestShootTargetFromCurrentPosition(
                man.Pawn,
                TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll |
                TargetScanFlags.NeedAutoTargetable)?.Thing ?? LocalTargetInfo.Invalid;
        }
    }

    public class DummyCaster : Thing, IFakeCaster
    {
        private readonly Pawn pawn;
        private readonly TurretVerb verb;

        public DummyCaster(Pawn pawn, TurretVerb verb)
        {
            this.pawn = pawn;
            this.verb = verb;
            def = ThingDef.Named("MVCF_Dummy");
        }

        public DummyCaster()
        {
        }

        public override Vector3 DrawPos => verb.DrawPos(verb.Target, pawn, pawn.DrawPos);

        public Thing RealCaster()
        {
            return pawn;
        }

        public override void Tick()
        {
            if (pawn == null) return;

            Position = pawn.Position;
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) Destroy();
        }
    }

    public class Verb_Search : Verb_LaunchProjectile
    {
        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg,
            bool surpriseAttack = false,
            bool canHitNonTargetPawns = true)
        {
            return false;
        }

        protected override bool TryCastShot()
        {
            return false;
        }
    }
}