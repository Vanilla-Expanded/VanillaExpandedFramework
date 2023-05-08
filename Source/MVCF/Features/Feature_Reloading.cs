using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_Reloading : Feature_VerbComps
{
    public override string Name => "Reloading";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_Reloading()).Append(new PatchSet_IntegratedToggle());
}
