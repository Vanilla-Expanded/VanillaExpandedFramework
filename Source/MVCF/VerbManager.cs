using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Features;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF;

public class VerbManager : IExposable
{
    private static readonly Dictionary<ThingWithComps, bool> preferMeleeCache =
        new();

    private readonly List<IVerbManagerComp> comps = new();

    private readonly List<ManagedVerb> drawVerbs = new();
    private readonly List<ManagedVerb> tickVerbs = new();
    public Verb CurrentVerb;
    public DebugOptions debugOpts;
    public bool HasVerbs;
    public Verb SearchVerb;
    private List<ManagedVerb> verbs = new();
    public bool NeedsTicking { get; private set; }

    public IEnumerable<ManagedVerb> CurrentlyUseableRangedVerbs => verbs.Where(v =>
        !v.Verb.IsMeleeAttack && v.Props is not { canFireIndependently: true } && v.Enabled &&
        v.Verb.Available() && (Pawn.IsColonist || v.Props is not { colonistOnly: true }));

    public bool ShouldBrawlerUpset => BrawlerHated.Any();

    public IEnumerable<Verb> BrawlerTolerates => ManagedVerbs.Where(mv => mv.Props is { brawlerCaresAbout: false }).Select(mv => mv.Verb).Concat(
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
        foreach (var verb in verbs) verb.Notify_Spawned();
    }

    public void Notify_Despawned()
    {
        foreach (var verb in verbs) verb.Notify_Despawned();
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
        comps.Clear();
        comps.AddRange(pawn.AllComps.OfType<IVerbManagerComp>());
        foreach (var comp in comps) comp.Initialize(this);
        if (Base.IsIgnoredMod(pawn?.def?.modContentPack?.Name)) return;
        if (!Base.GetFeature<Feature_RangedAnimals>().Enabled && pawn?.VerbTracker?.AllVerbs != null && pawn.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack))
            Log.ErrorOnce(
                $"[MVCF] Found pawn {pawn} with native ranged verbs while that feature is not enabled." +
                $" Enabling now. This is not recommended. Contact the author of {pawn?.def?.modContentPack?.Name} and ask them to add a MVCF.ModDef.",
                pawn?.def?.modContentPack?.Name?.GetHashCode() ?? -1);
        if (verbs.NullOrEmpty()) InitializeVerbs();
        foreach (var comp in comps) comp.PostInit();
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

        if (Pawn.Spawned) mv.Notify_Spawned();

        if (mv.NeedsDrawing)
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
        foreach (var comp in comps) comp.PostAdded(mv);
        RecalcSearchVerb();
    }

    public ManagedVerb ChooseVerb(LocalTargetInfo target, List<ManagedVerb> options)
    {
        ManagedVerb bestVerb = null;
        foreach (var comp in comps)
            if (comp.ChooseVerb(target, options, out bestVerb))
                return bestVerb;

        if (!target.IsValid || (Pawn.Map != null && !target.Cell.InBounds(Pawn.Map)))
        {
            Log.Error("[MVCF] ChooseVerb given invalid target with pawn " + Pawn + " and target " + target);
            if (debugOpts.ScoreLogging)
                Log.Error("(Current job is " + Pawn.CurJob + " with verb " + Pawn.CurJob?.verbToUse + " and target " +
                          Pawn.CurJob?.targetA + ")");
            return null;
        }

        var bestScore = 0f;
        foreach (var verb in options)
        {
            if (verb.Verb is IVerbScore verbScore && verbScore.ForceUse(Pawn, target)) return verb;
            var score = verb.GetScore(Pawn, target, debugOpts.ScoreLogging);
            if (debugOpts.ScoreLogging) Log.Message("Score is " + score + " compared to " + bestScore);
            if (score <= bestScore) continue;
            bestScore = score;
            bestVerb = verb;
        }

        if (debugOpts.ScoreLogging) Log.Message("ChooseVerb returning " + bestVerb);
        return bestVerb;
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

        foreach (var comp in comps) comp.PostRemoved(mv);

        RecalcSearchVerb();
    }

    public void RecalcSearchVerb()
    {
        if (debugOpts.VerbLogging) Log.Message("RecalcSearchVerb");
        var verbsToUse = verbs
            .Where(v => v.Enabled && v.Props is not { canFireIndependently: true } && !v.Verb.IsMeleeAttack)
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
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < drawVerbs.Count; i++) drawVerbs[i].DrawOn(Pawn, drawPos);
    }

    public void Tick()
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < tickVerbs.Count; i++) tickVerbs[i].Tick();
    }

    public IEnumerable<Verb> ExtraVerbsFor(ThingWithComps eq) => comps.SelectMany(comp => comp.ExtraVerbsFor(eq));
    public IEnumerable<Verb> ExtraVerbsFor(Apparel apparel) => comps.SelectMany(comp => comp.ExtraVerbsFor(apparel));
    public IEnumerable<Verb> ExtraVerbsFor(Hediff hediff) => comps.SelectMany(comp => comp.ExtraVerbsFor(hediff));
    public IEnumerable<Verb> ExtraVerbsFor(Thing item) => comps.SelectMany(comp => comp.ExtraVerbsFor(item));
}

public enum VerbSource
{
    None,
    Apparel,
    Equipment,
    Hediff,
    RaceDef,
    Inventory
}

public interface IVerbManagerComp
{
    bool ChooseVerb(LocalTargetInfo target, List<ManagedVerb> verbs, out ManagedVerb verb);
    void PostInit();
    void Initialize(VerbManager parent);
    void PostAdded(ManagedVerb verb);
    void PostRemoved(ManagedVerb verb);
    IEnumerable<Verb> ExtraVerbsFor(ThingWithComps eq);
    IEnumerable<Verb> ExtraVerbsFor(Apparel apparel);
    IEnumerable<Verb> ExtraVerbsFor(Hediff hediff);
    IEnumerable<Verb> ExtraVerbsFor(Thing item);
}

public abstract class VerbManagerComp : ThingComp, IVerbManagerComp
{
    public VerbManager Manager;

    public virtual bool ChooseVerb(LocalTargetInfo target, List<ManagedVerb> verbs, out ManagedVerb verb)
    {
        verb = null;
        return false;
    }

    public virtual void PostInit()
    {
    }

    public void Initialize(VerbManager parent)
    {
        Manager = parent;
    }

    public virtual void PostAdded(ManagedVerb verb)
    {
    }

    public virtual void PostRemoved(ManagedVerb verb)
    {
    }

    public virtual IEnumerable<Verb> ExtraVerbsFor(ThingWithComps eq)
    {
        yield break;
    }

    public virtual IEnumerable<Verb> ExtraVerbsFor(Apparel apparel)
    {
        yield break;
    }

    public virtual IEnumerable<Verb> ExtraVerbsFor(Hediff hediff)
    {
        yield break;
    }

    public virtual IEnumerable<Verb> ExtraVerbsFor(Thing item)
    {
        yield break;
    }
}