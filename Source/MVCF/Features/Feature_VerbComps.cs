using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_VerbComps : Feature_Humanoid
{
    public override string Name => "VerbComps";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_VerbComps());
}
