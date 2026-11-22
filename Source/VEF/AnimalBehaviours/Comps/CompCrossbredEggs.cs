using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours;

public sealed class CompCrossbredEggs : ThingComp
{
    // A missing list represents ordinary eggs and saves made before per-egg kinds.
    private List<PawnKindDef> hatchlingKinds;

    internal void SetKinds(List<PawnKindDef> kinds) => hatchlingKinds = kinds;

    internal PawnKindDef KindAt(int index, PawnKindDef fallback)
    {
        return hatchlingKinds != null && index >= 0 && index < hatchlingKinds.Count
            ? hatchlingKinds[index] ?? fallback
            : fallback;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref hatchlingKinds, "vefCrossbredHatchlingKinds", LookMode.Def);
    }

    public override void PostSplitOff(Thing piece)
    {
        if (piece == parent || hatchlingKinds == null ||
            piece.TryGetComp<CompCrossbredEggs>() is not { } split)
            return;

        // SplitOff has already reduced the original count. Move the tail to the new stack.
        var kinds = EnsureKinds(parent.stackCount + piece.stackCount);
        split.hatchlingKinds = kinds.GetRange(parent.stackCount, piece.stackCount);
        kinds.RemoveRange(parent.stackCount, piece.stackCount);
    }

    public override void PreAbsorbStack(Thing otherStack, int count)
    {
        if (count <= 0 || otherStack == parent ||
            otherStack.TryGetComp<CompCrossbredEggs>() is not { } source ||
            (hatchlingKinds == null && source.hatchlingKinds == null))
            return;

        // Both counts are still unchanged. Move only the eggs that fit in the receiver.
        var destinationKinds = EnsureKinds(parent.stackCount);
        var sourceKinds = source.EnsureKinds(otherStack.stackCount);
        int firstMoved = otherStack.stackCount - count;
        destinationKinds.AddRange(sourceKinds.GetRange(firstMoved, count));
        sourceKinds.RemoveRange(firstMoved, count);
    }

    private List<PawnKindDef> EnsureKinds(int count)
    {
        hatchlingKinds ??= new List<PawnKindDef>(count);
        if (hatchlingKinds.Count > count)
            hatchlingKinds.RemoveRange(count, hatchlingKinds.Count - count);

        var fallback = parent.TryGetComp<CompHatcher>().Props.hatcherPawn;
        while (hatchlingKinds.Count < count)
            hatchlingKinds.Add(fallback);
        return hatchlingKinds;
    }

    [StaticConstructorOnStartup]
    private static class Register
    {
        static Register()
        {
            // Defs are resolved here, and new, loaded and split eggs all get the same comps.
            var properties = new CompProperties { compClass = typeof(CompCrossbredEggs) };
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.GetCompProperties<CompProperties_Hatcher>() != null &&
                    !def.comps.Exists(comp => comp.compClass == typeof(CompCrossbredEggs)))
                    def.comps.Add(properties);
            }
        }
    }
}
