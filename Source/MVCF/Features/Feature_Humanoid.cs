using System.Collections.Generic;
using MVCF.PatchSets;

namespace MVCF.Features;

public abstract class Feature_Humanoid : Feature
{
    public override IEnumerable<PatchSet> GetPatchSets()
    {
        foreach (var patchSet in base.GetPatchSets()) yield return patchSet;
        yield return new PatchSet_HumanoidGizmos();
        yield return new PatchSet_Brawlers();
        yield return new PatchSet_Hunting();
    }
}
