using System.Collections.Generic;
using MVCF.PatchSets;
using MVCF.PatchSets.Trackers;

namespace MVCF.Features;

public abstract class Feature_MultiVerb : Feature
{
    public override IEnumerable<PatchSet> GetPatchSets()
    {
        foreach (var patchSet in base.GetPatchSets()) yield return patchSet;

        yield return new PatchSet_MultiVerb();
        yield return new PatchSet_BatteLog();
        yield return new PatchSet_TargetFinder();
        yield return new PatchSet_Equipment();
        yield return new PatchSet_Stats();
        yield return new PatchSet_InfoCard();
    }
}
