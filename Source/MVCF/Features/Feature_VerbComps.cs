using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;
using MVCF.PatchSets.Trackers;

namespace MVCF.Features;

public class Feature_VerbComps : Feature
{
    public override string Name => "VerbComps";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_Equipment()).Append(new PatchSet_VerbComps());
}
