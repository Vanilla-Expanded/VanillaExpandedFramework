using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Features.PatchSets;
using MVCF.Utilities;
using Verse;

namespace MVCF.Features
{
    public abstract class Feature
    {
        private static readonly HashSet<Patch> appliedPatches = new();
        public bool Enabled;
        public abstract string Name { get; }
        public IEnumerable<Patch> Patches => GetPatches().Concat(GetPatchSets().SelectMany(set => set.GetPatches()));

        public virtual IEnumerable<Patch> GetPatches()
        {
            yield return Patch_Pawn_TryGetAttackVerb.GetPatch();
            foreach (var patch in TargetFinder.GetPatches()) yield return patch;
        }

        public virtual IEnumerable<PatchSet> GetPatchSets()
        {
            yield return new PatchSet_BatteLog();
            yield return new PatchSet_Base();
            if (ModLister.HasActiveModWithName("RunAndGun")) yield return new PatchSet_RunAndGun();
        }

        public virtual void Enable(Harmony harm)
        {
            Enabled = true;
            foreach (var patch in Patches)
                if (!appliedPatches.Contains(patch))
                {
                    patch.Apply(harm);
                    appliedPatches.Add(patch);
                }
        }

        public virtual void Disable(Harmony harm)
        {
            Enabled = false;
            foreach (var patch in Patches)
                if (appliedPatches.Contains(patch))
                {
                    patch.Unapply(harm);
                    appliedPatches.Remove(patch);
                }
        }
    }
}