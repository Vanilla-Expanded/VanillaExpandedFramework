using System.Collections.Generic;
using MVCF.Features.PatchSets;

namespace MVCF.Features
{
    public class Feature_IndependentVerbs : Feature
    {
        public override string Name => "IndependentFire";

        public override IEnumerable<PatchSet> GetPatchSets()
        {
            foreach (var patchSet in base.GetPatchSets()) yield return patchSet;

            yield return new PatchSet_IndependentVerbs();
        }
    }
}