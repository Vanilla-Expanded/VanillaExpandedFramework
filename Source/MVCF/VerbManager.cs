using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF
{
    public class VerbManager : IVerbOwner
    {
        private static readonly Dictionary<ThingWithComps, bool> preferMeleeCache =
            new Dictionary<ThingWithComps, bool>();

        private readonly List<ManagedVerb> drawVerbs = new List<ManagedVerb>();
        public readonly List<TurretVerb> tickVerbs = new List<TurretVerb>();
        private readonly List<ManagedVerb> verbs = new List<ManagedVerb>();
        public Verb CurrentVerb;
        public DebugOptions debugOpts;
        public bool HasVerbs;
        public Verb OverrideVerb;
        public Verb SearchVerb;
        public bool NeedsTicking { get; private set; }

        public IEnumerable<ManagedVerb> CurrentlyUseableRangedVerbs => verbs.Where(v =>
            !v.Verb.IsMeleeAttack && (v.Props == null || !v.Props.canFireIndependently) && v.Enabled &&
            v.Verb.Available() && !(!Pawn.IsColonist && v.Props != null && !v.Props.colonistOnly));

        public bool ShouldBrawlerUpset
        {
            get
            {
                var prim = Pawn.equipment.Primary;
                var eq = Pawn.equipment.PrimaryEq;
                var exclude = PreferMelee(prim)
                    ? eq?.AllVerbs.Where(v => !v.IsMeleeAttack) ?? new List<Verb>()
                    : new List<Verb>();
                return AllRangedVerbs.Except(exclude).Any();
            }
        }

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

        public static bool PreferMelee(ThingWithComps eq)
        {
            if (preferMeleeCache.TryGetValue(eq, out var res)) return res;

            res = (eq.TryGetComp<CompEquippable>()?.props as CompProperties_VerbProps ??
                   eq.TryGetComp<Comp_VerbProps>()?.Props)?.ConsiderMelee ?? false;
            preferMeleeCache.Add(eq, res);
            return res;
        }

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
            if (!Base.Features.RangedAnimals && !Base.IgnoredFeatures.RangedAnimals &&
                pawn?.VerbTracker?.AllVerbs != null &&
                pawn.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack) &&
                !Base.IsIgnoredMod(pawn?.def?.modContentPack?.Name))
            {
                Log.ErrorOnce(
                    "[MVCF] Found pawn with native ranged verbs while that feature is not enabled. Enabling now. This is not recommended. Contact the author of " +
                    pawn?.def?.modContentPack?.Name + " and ask them to add a MVCF.ModDef.",
                    pawn?.def?.modContentPack?.Name?.GetHashCode() ?? -1);
                Base.Features.RangedAnimals = true;
                Base.ApplyPatches();
            }

            if (!Base.IsIgnoredMod(pawn?.def?.modContentPack?.Name) && pawn?.VerbTracker?.AllVerbs != null &&
                Base.Features.RangedAnimals && !Base.IgnoredFeatures.RangedAnimals)
                foreach (var verb in pawn.VerbTracker.AllVerbs)
                    AddVerb(verb, VerbSource.RaceDef, pawn.TryGetComp<Comp_VerbGiver>()?.PropsFor(verb));

            if (pawn?.health?.hediffSet?.hediffs != null && !Base.IgnoredFeatures.HediffVerbs)
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                    this.AddVerbs(hediff);

            if (pawn?.apparel?.WornApparel != null && !Base.IgnoredFeatures.ApparelVerbs)
                foreach (var apparel in pawn.apparel.WornApparel)
                    this.AddVerbs(apparel);

            if (pawn?.equipment?.AllEquipmentListForReading != null && !Base.IgnoredFeatures.ExtraEquipmentVerbs)
                foreach (var eq in pawn.equipment.AllEquipmentListForReading)
                    this.AddVerbs(eq);
        }

        public void AddVerb(Verb verb, VerbSource source, AdditionalVerbProps props)
        {
            if (debugOpts.VerbLogging) Log.Message("Adding " + verb + " from " + source + " with props " + props);
            if (AllVerbs.Contains(verb))
            {
                if (debugOpts.VerbLogging) Log.Warning("Added duplicate verb " + verb);
                return;
            }

            ManagedVerb mv;
            if (props != null && props.canFireIndependently)
            {
                TurretVerb tv;
                if (props.managedClass != null)
                    tv = (TurretVerb) Activator.CreateInstance(props.managedClass, verb, source, props, this);
                else tv = new TurretVerb(verb, source, props, this);
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
                if (props?.managedClass != null)
                    mv = (ManagedVerb) Activator.CreateInstance(props.managedClass, verb, source, props, this);
                else
                    mv = new ManagedVerb(verb, source, props, this);
            }

            if (props != null && props.draw) drawVerbs.Add(mv);

            verbs.Add(mv);
            RecalcSearchVerb();
        }

        public void RemoveVerb(Verb verb)
        {
            if (debugOpts.VerbLogging) Log.Message("Removing " + verb);
            var mv = verbs.Find(m => m.Verb == verb);
            if (debugOpts.VerbLogging) Log.Message("Found ManagedVerb: " + mv);

            var success = verbs.Remove(mv);
            if (debugOpts.VerbLogging) Log.Message("Succeeded at removing: " + success);
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

    public enum VerbSource
    {
        Apparel,
        Equipment,
        Hediff,
        RaceDef
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