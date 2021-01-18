using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Harmony;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF
{
    public class VerbManager : IVerbOwner
    {
        private readonly List<ManagedVerb> drawVerbs = new List<ManagedVerb>();
        private readonly List<TurretVerb> tickVerbs = new List<TurretVerb>();
        private readonly List<ManagedVerb> verbs = new List<ManagedVerb>();
        public Verb CurrentVerb;
        public bool HasVerbs;
        public Verb SearchVerb = new Verb_Search();
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
                verbClass = typeof(Verb_Shoot),
                label = Base.SearchLabel,
                defaultProjectile = ThingDef.Named("Bullet_Revolver"),
                onlyManualCast = true
            }
        };

        public List<Tool> Tools => new List<Tool>();
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;
        public Thing ConstantCaster => Pawn;

        public ManagedVerb GetManagedVerbForVerb(Verb verb)
        {
            var mv = verbs.FirstOrFallback(v => v.Verb == verb);
            if (mv == null)
            {
                Log.Warning("[MVCF] Attempted to get ManagedVerb for verb " + verb.Label() +
                            " which does not have one. This may cause issues.");
                Log.Warning("All ManagedVerbs:");
                foreach (var v in verbs) Log.Warning("  " + v.Verb.Label());
            }

            return mv;
        }

        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            VerbTracker = new VerbTracker(this);
            SearchVerb = VerbTracker.PrimaryVerb;
            NeedsTicking = false;
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
                    if (comp == null)
                    {
                        var extComp = eq.TryGetComp<Comp_VerbGiver>();
                        if (extComp == null) continue;
                        foreach (var verb in extComp.VerbTracker.AllVerbs)
                            AddVerb(verb, VerbSource.Equipment, extComp.PropsFor(verb));
                    }
                    else
                    {
                        foreach (var verb in comp.VerbTracker.AllVerbs)
                            AddVerb(verb, VerbSource.Equipment, null);
                    }
                }
        }


        public void AddVerb(Verb verb, VerbSource source, AdditionalVerbProps props)
        {
            // Log.Message("AddVerb " + verb + ", " + source + ", " + props);
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
            var verbsToUse = verbs
                .Where(v => v.Enabled && (v.Props == null || !v.Props.canFireIndependently)).ToList();
            if (verbsToUse.Count == 0)
            {
                HasVerbs = false;
                return;
            }

            HasVerbs = true;

            SearchVerb.verbProps.range = verbsToUse.Select(v => v.Verb.verbProps.range).Max();
            SearchVerb.verbProps.minRange = verbsToUse.Select(v => v.Verb.verbProps.minRange).Min();
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
        private readonly VerbManager man;
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
        }

        public void Toggle()
        {
            Enabled = !Enabled;
            man.RecalcSearchVerb();
        }

        public void DrawOn(Pawn p, Vector3 drawPos)
        {
            if (Props == null) return;
            if (!Props.draw) return;
            if (p.Dead || !p.Spawned) return;
            drawPos += Vector3.up;
            var target = PointingTarget(p);
            if (target != null && target.IsValid)
            {
                var a = target.HasThing ? target.Thing.DrawPos : target.Cell.ToVector3Shifted();

                DrawPointingAt(Props.DrawPos(p.def.defName, drawPos, p.Rotation),
                    (a - drawPos).MagnitudeHorizontalSquared() > 0.001f ? (a - drawPos).AngleFlat() : 0f, p.BodySize);
            }
            else
            {
                DrawPointingAt(Props.DrawPos(p.def.defName, drawPos, p.Rotation), p.Rotation.AsAngle, p.BodySize);
            }
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
        private readonly Pawn pawn;
        private int cooldownTicksLeft;
        private LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
        private int warmUpTicksLeft;


        public TurretVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man) : base(verb, source,
            props, man)
        {
            pawn = verb.CasterPawn;
            dummyCaster = new DummyCaster(pawn);
            dummyCaster.Tick();
            dummyCaster.SpawnSetup(pawn.Map, false);
            verb.caster = dummyCaster;
            verb.castCompleteCallback = () => cooldownTicksLeft = Verb.verbProps.AdjustedCooldownTicks(Verb, pawn);
        }

        public void Tick()
        {
            // Log.Message("TurretVerb Tick:");
            // Log.Message("  Bursting: " + Verb.Bursting);
            // Log.Message("  cooldown: " + cooldownTicksLeft);
            // Log.Message("  warmup: " + warmUpTicksLeft);
            // Log.Message("  currentTarget: " + currentTarget);
            Verb.VerbTick();
            if (Verb.Bursting) return;
            if (cooldownTicksLeft > 0) cooldownTicksLeft--;

            if (cooldownTicksLeft > 0) return;
            if (!currentTarget.IsValid || currentTarget.HasThing && currentTarget.ThingDestroyed)
            {
                // Log.Message("Attempting to find a target");
                var man = pawn.Manager();
                var sv = man.SearchVerb;
                man.SearchVerb = Verb;
                currentTarget = (LocalTargetInfo) (Thing) AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn,
                    TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll |
                    TargetScanFlags.NeedAutoTargetable);
                man.SearchVerb = sv;
                TryStartCast();
            }
            else if (warmUpTicksLeft == 0)
            {
                // Log.Message("Starting cast!");
                TryCast();
            }
            else if (warmUpTicksLeft > 0)
            {
                // Log.Message("Still warming up");
                warmUpTicksLeft--;
            }
            else
            {
                // Log.Message("Firing again");
                TryStartCast();
            }
        }

        private void TryStartCast()
        {
            if (Verb.verbProps.warmupTime > 0)
                warmUpTicksLeft = (Verb.verbProps.warmupTime * pawn.GetStatValue(StatDefOf.AimingDelayFactor))
                    .SecondsToTicks();
            else
                TryCast();
        }

        private void TryCast()
        {
            warmUpTicksLeft = -1;
            var success = Verb.TryStartCastOn(currentTarget);
            Log.Message(pawn + (success ? " successfully " : " failed to ") + "fire at " + currentTarget);
        }

        public override LocalTargetInfo PointingTarget(Pawn p)
        {
            return currentTarget;
        }
    }

    public class DummyCaster : Thing, IFakeCaster
    {
        private readonly Pawn pawn;

        public DummyCaster(Pawn pawn)
        {
            this.pawn = pawn;
            def = ThingDef.Named("MVCF_Dummy");
        }

        public DummyCaster()
        {
        }

        public override Vector3 DrawPos => pawn.DrawPos;

        public Thing RealCaster()
        {
            return pawn;
        }

        public override void Tick()
        {
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