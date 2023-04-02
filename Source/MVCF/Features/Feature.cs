using System.Collections.Generic;
using System.Linq;
using MVCF.ModCompat;
using MVCF.PatchSets;
using Verse;

namespace MVCF.Features;

public abstract class Feature
{
    public bool Enabled;
    public abstract string Name { get; }
    public IEnumerable<Patch> Patches => GetPatchSets().SelectMany(set => set.GetPatches());

    public virtual IEnumerable<PatchSet> GetPatchSets()
    {
        yield return new PatchSet_Base();
        yield return new PatchSet_MultiVerb();
        yield return new PatchSet_BatteLog();
        yield return new PatchSet_TargetFinder();
        yield return new PatchSet_Equipment();
        // yield return new PatchSet_Debug();
        if (ModLister.HasActiveModWithName("RunAndGun")) yield return new PatchSet_RunAndGun();
        if (DualWieldCompat.DoNullCheck) yield return new PatchSet_DualWield();
    }
}
