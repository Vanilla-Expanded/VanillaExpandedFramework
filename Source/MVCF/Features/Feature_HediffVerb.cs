using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets.Trackers;

namespace MVCF.Features;

public class Feature_HediffVerb : Feature_Humanoid
{
    public override string Name => "HediffVerbs";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_Hediffs());
}
