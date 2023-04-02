using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_RangedAnimals : Feature_MultiVerb
{
    public override string Name => "RangedAnimals";
    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_Animals());
}
