using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Features;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF
{
    public class VerbManager : IVerbOwner
    {
        private static readonly Dictionary<ThingWithComps, bool> preferMeleeCache =
            new();

        private readonly List<ManagedVerb> drawVerbs = new();
        private readonly List<ManagedVerb> tickVerbs = new();
        private readonly List<ManagedVerb> verbs = new();
        public Verb CurrentVerb;
        public DebugOptions debugOpts;
        public bool HasVerbs;
        public Verb SearchVerb;
        public bool NeedsTicking { get; private set; }

        public IEnumerable<ManagedVerb> CurrentlyUseableRangedVerbs => verbs.Where(v =>
            !v.Verb.IsMeleeAttack && v.Props is not {canFireIndependently: true} && v.Enabled &&
            v.Verb.Available() && (Pawn.IsColonist || v.Props is not {colonistOnly: true}));

        public bool ShouldBrawlerUpset => BrawlerHated.Any();

        public IEnumerable<Verb> BrawlerTolerates => ManagedVerbs.Where(mv => mv.Props is {brawlerCaresAbout: false}).Select(mv => mv.Verb).Concat(
            PreferMelee(Pawn.equipment.Primary)
                ? Pawn.equipment.PrimaryEq?.AllVerbs.Where(v => !v.IsMeleeAttack) ?? new List<Verb>()
                : new List<Verb>());

        public IEnumerable<Verb> BrawlerHated => AllRangedVerbs.Except(BrawlerTolerates);

        public IEnumerable<Verb> AllVerbs => verbs.Select(mv => mv.Verb);
        public IEnumerable<Verb> AllRangedVerbs => verbs.Select(mv => mv.Verb).Where(verb => !verb.IsMeleeAttack);

        public IEnumerable<Verb> AllRangedVerbsNoEquipment =>
            verbs.Where(mv => mv.Source != VerbSource.Equipment).Select(mv => mv.Verb);

        public IEnumerable<ManagedVerb> ManagedVerbs => verbs;

        public IEnumerable<Verb> AllRangedVerbsNoEquipmentNoApparel => verbs
            .Where(mv => mv.Source != VerbSource.Equipment && mv.Source != VerbSource.Apparel).Select(mv => mv.Verb);

        public Pawn Pawn { get; private set; }

        public string UniqueVerbOwnerID() => "VerbManager_" + (Pawn as IVerbOwner).UniqueVerbOwnerID();

        public bool VerbsStillUsableBy(Pawn p) => p == Pawn;

        public VerbTracker VerbTracker { get; private set; }

        public List<VerbProperties> VerbProperties => new();

        public List<Tool> Tools => new();
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;
        public Thing ConstantCaster => Pawn;

        public void Notify_Spawned()
        {
            foreach (var tv in tickVerbs.OfType<TurretVerb>()) tv.CreateCaster();
        }

        public void Notify_Despawned()
        {
            foreach (var tv in tickVerbs.OfType<TurretVerb>()) tv.DestroyCaster();
        }

        public static bool PreferMelee(ThingWithComps eq)
        {
            if (eq == null) return false;
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
                              " which does not have one. This may cause issues.", verb.GetHashCode());

            return mv;
        }

        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            VerbTracker = new VerbTracker(this);
            NeedsTicking = false;
            debugOpts.ScoreLogging = false;
            debugOpts.VerbLogging = false;
            if (Base.IsIgnoredMod(pawn?.def?.modContentPack?.Name)) return;
            if (!Base.GetFeature<Feature_RangedAnimals>().Enabled && pawn?.VerbTracker?.AllVerbs != null && pawn.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack))
                Log.ErrorOnce(
                    $"[MVCF] Found pawn {pawn} with native ranged verbs while that feature is not enabled." +
                    $" Enabling now. This is not recommended. Contact the author of {pawn?.def?.modContentPack?.Name} and ask them to add a MVCF.ModDef.",
                    pawn?.def?.modContentPack?.Name?.GetHashCode() ?? -1);

            if (pawn?.VerbTracker?.AllVerbs != null && Base.GetFeature<Feature_RangedAnimals>().Enabled)
                foreach (var verb in pawn.VerbTracker.AllVerbs)
                    AddVerb(verb, VerbSource.RaceDef, pawn.TryGetComp<Comp_VerbProps>()?.PropsFor(verb));

            if (pawn?.health?.hediffSet?.hediffs != null && Base.GetFeature<Feature_HediffVerb>().Enabled)
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                    this.AddVerbs(hediff);

            if (pawn?.apparel?.WornApparel != null && Base.GetFeature<Feature_ApparelVerbs>().Enabled)
                foreach (var apparel in pawn.apparel.WornApparel)
                    this.AddVerbs(apparel);

            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                if (Base.GetFeature<Feature_ExtraEquipmentVerbs>().Enabled)
                    foreach (var eq in pawn.equipment.AllEquipmentListForReading)
                        this.AddVerbs(eq);
                else if (pawn.equipment.Primary is { } eq) this.AddVerbs(eq);
            }
        }

        public void AddVerb(Verb verb, VerbSource source, AdditionalVerbProps props)
        {
            if (debugOpts.VerbLogging) Log.Message("Adding " + verb + " from " + source + " with props " + props);
            if (AllVerbs.Contains(verb))
            {
                if (debugOpts.VerbLogging) Log.Warning("Added duplicate verb " + verb);
                return;
            }

            var mv = props switch
            {
                {managedClass: { } type} => (ManagedVerb) Activator.CreateInstance(type, verb, source, props, this),
                {canFireIndependently: true} => new TurretVerb(verb, source, props, this),
                {draw: true} => new DrawnVerb(verb, source, props, this),
                _ => new ManagedVerb(verb, source, props, this)
            };

            if (Pawn.Spawned && mv is TurretVerb tv) tv.CreateCaster();

            if (props is {draw: true})
                if (mv.NeedsTicking)
                {
                    if (tickVerbs.Count == 0)
                    {
                        NeedsTicking = true;
                        WorldComponent_MVCF.GetComp().TickManagers.Add(new System.WeakReference<VerbManager>(this));
                    }

                    tickVerbs.Add(mv);
                }

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
            if (!success) return;
            if (drawVerbs.Contains(mv)) drawVerbs.Remove(mv);
            if (tickVerbs.Contains(mv) && tickVerbs.Remove(mv) && tickVerbs.Count == 0)
            {
                NeedsTicking = false;
                WorldComponent_MVCF.GetComp().TickManagers.RemoveAll(wr =>
                {
                    if (!wr.TryGetTarget(out var man)) return true;
                    return man == this;
                });
            }

            RecalcSearchVerb();
        }

        public void RecalcSearchVerb()
        {
            if (debugOpts.VerbLogging) Log.Message("RecalcSearchVerb");
            var verbsToUse = verbs
                .Where(v => v.Enabled && v.Props is not {canFireIndependently: true} && !v.Verb.IsMeleeAttack)
                .ToList();
            if (debugOpts.VerbLogging) verbsToUse.ForEach(v => Log.Message("Verb: " + v.Verb));
            if (verbsToUse.Count == 0)
            {
                HasVerbs = false;
                if (debugOpts.VerbLogging) Log.Message("No Verbs");
                return;
            }

            HasVerbs = true;
            SearchVerb = verbsToUse.MaxBy(verb => verb.Verb.verbProps.range)?.Verb;
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
}