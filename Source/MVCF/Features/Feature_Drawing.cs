using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_Drawing : Feature
{
    public override string Name => "Drawing";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_Drawing());
}
