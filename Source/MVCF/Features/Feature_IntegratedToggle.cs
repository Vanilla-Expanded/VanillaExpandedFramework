using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_IntegratedToggle : Feature
{
    public override string Name => "IntegratedToggle";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_IntegratedToggle());
}
