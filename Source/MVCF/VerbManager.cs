using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Features;
using MVCF.Utilities;
using UnityEngine;
using Verse;

namespace MVCF
{
    public class VerbManager : IExposable
    {
        private static readonly Dictionary<ThingWithComps, bool> preferMeleeCache =
            new();

        private readonly List<ManagedVerb> drawVerbs = new();
        private readonly List<ManagedVerb> tickVerbs = new();
        public Verb CurrentVerb;
        public DebugOptions debugOpts;
        public bool HasVerbs;
        public Verb SearchVerb;
        private List<ManagedVerb> verbs = new();
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

        public Pawn Pawn { get; private set; }

        public void ExposeData()
        {
            Scribe_References.Look(ref CurrentVerb, "currentVerb");
            Scribe_Collections.Look(ref verbs, "verbs", LookMode.Reference);
        }

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

        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            NeedsTicking = false;
            debugOpts.ScoreLogging = false;
            debugOpts.VerbLogging = false;
            if (Base.IsIgnoredMod(pawn?.def?.modContentPack?.Name)) return;
            if (!Base.GetFeature<Feature_RangedAnimals>().Enabled && pawn?.VerbTracker?.AllVerbs != null && pawn.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack))
                Log.ErrorOnce(
                    $"[MVCF] Found pawn {pawn} with native ranged verbs while that feature is not enabled." +
                    $" Enabling now. This is not recommended. Contact the author of {pawn?.def?.modContentPack?.Name} and ask them to add a MVCF.ModDef.",
                    pawn?.def?.modContentPack?.Name?.GetHashCode() ?? -1);
            if (verbs.NullOrEmpty()) InitializeVerbs();

            RecalcSearchVerb();
        }

        public void InitializeVerbs()
        {
            if (Pawn?.VerbTracker?.AllVerbs != null && Base.GetFeature<Feature_RangedAnimals>().Enabled)
                foreach (var verb in Pawn.VerbTracker.AllVerbs)
                    AddVerb(verb, VerbSource.RaceDef);

            if (Pawn?.health?.hediffSet?.hediffs != null && Base.GetFeature<Feature_HediffVerb>().Enabled)
                foreach (var hediff in Pawn.health.hediffSet.hediffs)
                    this.AddVerbs(hediff);

            if (Pawn?.apparel?.WornApparel != null && Base.GetFeature<Feature_ApparelVerbs>().Enabled)
                foreach (var apparel in Pawn.apparel.WornApparel)
                    this.AddVerbs(apparel);

            if (Pawn?.equipment?.AllEquipmentListForReading != null)
            {
                if (Base.GetFeature<Feature_ExtraEquipmentVerbs>().Enabled)
                    foreach (var eq in Pawn.equipment.AllEquipmentListForReading)
                        this.AddVerbs(eq);
                else if (Pawn.equipment.Primary is { } eq) this.AddVerbs(eq);
            }
        }

        public void AddVerb(Verb verb, VerbSource source)
        {
            if (debugOpts.VerbLogging) Log.Message($"Adding {verb} from {source}");

            if (AllVerbs.Contains(verb))
            {
                if (debugOpts.VerbLogging) Log.Warning("Added duplicate verb " + verb);
                return;
            }

            var mv = verb.Managed();

            mv.Notify_Added(this, source);

            if (Pawn.Spawned && mv is TurretVerb tv) tv.CreateCaster();

            if (mv.Props is {draw: true})
                drawVerbs.Add(mv);
            if (mv.NeedsTicking)
            {
                if (tickVerbs.Count == 0)
                {
                    NeedsTicking = true;
                    WorldComponent_MVCF.Instance.TickManagers.Add(new System.WeakReference<VerbManager>(this));
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

            mv.Notify_Removed();
            var success = verbs.Remove(mv);
            if (debugOpts.VerbLogging) Log.Message("Succeeded at removing: " + success);
            if (!success) return;
            if (drawVerbs.Contains(mv)) drawVerbs.Remove(mv);
            if (tickVerbs.Contains(mv) && tickVerbs.Remove(mv) && tickVerbs.Count == 0)
            {
                NeedsTicking = false;
                WorldComponent_MVCF.Instance.TickManagers.RemoveAll(wr =>
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
        None,
        Apparel,
        Equipment,
        Hediff,
        RaceDef
    }
}