using System.Collections.Generic;
using Verse;

namespace VEF.Buildings;

public class ResearchBuildingExtension : DefModExtension, IMergeable
{
    // List of all research benches (if any) that are treated as equivalent to this one and can be used as replacements.
    public List<ThingDef> equivalentBenches;
    // List of all research facilities (if any) that are treated as equivalent to this one and can be used as replacements.
    public List<ThingDef> equivalentFacilities;

    // Priority is meaningless for this extension
    public float Priority => 0;

    public void Merge(object extension)
    {
        var other = (ResearchBuildingExtension)extension;

        if (!other.equivalentBenches.NullOrEmpty())
        {
            equivalentBenches ??= [];
            equivalentBenches.AddRangeUnique(other.equivalentBenches);
        }

        if (!equivalentFacilities.NullOrEmpty())
        {
            equivalentFacilities ??= [];
            equivalentFacilities.AddRangeUnique(other.equivalentFacilities);
        }
    }

    public bool CanMerge(object other) => other != null && other.GetType() == typeof(ResearchBuildingExtension);
}