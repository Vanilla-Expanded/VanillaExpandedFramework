using System.Collections.Generic;
using MVCF.PatchSets;
using Verse;

namespace MVCF.Features;

public class Feature_IntegratedToggle : Feature
{
    public override string Name => "IntegratedToggle";

    public override IEnumerable<PatchSet> GetPatchSets() => Gen.YieldSingle(new PatchSet_IntegratedToggle());
}
