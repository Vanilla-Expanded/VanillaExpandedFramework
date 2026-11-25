using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Buildings;

public class AffectedByFacilitiesExtension : DefModExtension
{
    // A list of Affected By Facilities defs that we're copying links from
    public List<ThingDef> copyLinksFrom = null;

    // If true, prevents any AffectedByFacilitiesExtension with copyFacilitiesFrom from linking with this building.
    public bool disableFacilityExtensionLinking = false;
    // A list of facilities that this building won't automatically link to
    public List<ThingDef> preventAutoLinkingWith = null;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (copyLinksFrom != null && parentDef is ThingDef def)
        {
            var parentComp = def.GetCompProperties<CompProperties_AffectedByFacilities>();
            if (parentComp != null)
            {
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    parentComp.linkableFacilities ??= [];

                    foreach (var otherDef in copyLinksFrom)
                    {
                        var otherFacilities = otherDef.GetCompProperties<CompProperties_AffectedByFacilities>()?.linkableFacilities;
                        if (otherFacilities == null)
                            continue;

                        foreach (var facility in otherFacilities)
                        {
                            // Don't link with specified facilities
                            if (preventAutoLinkingWith.NotNullAndContains(facility))
                                continue;
                            // Don't double link
                            if (parentComp.linkableFacilities.Contains(facility))
                                continue;
                            // Make sure that we're trying to link with a facility
                            var comp = facility.GetCompProperties<CompProperties_Facility>();
                            if (comp == null)
                                continue;
                            // If the facility is marked as "don't link" (globally or with this building), skip it
                            if (facility.GetModExtension<FacilityExtension>()?.RefusesAutoLinkFrom(def) == true)
                                continue;

                            // Add linking to both
                            comp.linkableBuildings ??= [];
                            comp.linkableBuildings.AddDistinct(def);
                            parentComp.linkableFacilities.Add(facility);
                        }
                    }
                });
            }
        }
    }

    // True if a facility copying links via FacilityExtension.copyLinksFrom should not link with this building
    public bool RefusesAutoLinkFrom(ThingDef facility) => disableFacilityExtensionLinking || preventAutoLinkingWith.NotNullAndContains(facility);
}