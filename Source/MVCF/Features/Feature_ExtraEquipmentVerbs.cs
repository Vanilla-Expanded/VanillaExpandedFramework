using System.Collections.Generic;
using System.Linq;
using MVCF.PatchSets;

namespace MVCF.Features;

public class Feature_ExtraEquipmentVerbs : Feature_Humanoid
{
    public override string Name => "ExtraEquipmentVerbs";

    public override IEnumerable<PatchSet> GetPatchSets() => base.GetPatchSets().Append(new PatchSet_PreferMelee()).Append(new PatchSet_ExtraEquipment());
}
