using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_IndependentVerbs : Feature
{
    public override string Name => "IndependentFire";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_IndependentVerbs());
}
