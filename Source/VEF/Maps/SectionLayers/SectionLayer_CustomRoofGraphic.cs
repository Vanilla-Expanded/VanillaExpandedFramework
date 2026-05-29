using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Maps;

[StaticConstructorOnStartup]
public class SectionLayer_CustomRoofGraphic : SectionLayer
{
    private static readonly bool anyRoofUsesCustomGraphic = DefDatabase<RoofDef>.AllDefs.Any(def => def.GetModExtension<RoofExtension>()?.EverUsesCustomRoofGraphic == true);

    public SectionLayer_CustomRoofGraphic(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlagDefOf.Roofs;
    }

    public override bool Visible => anyRoofUsesCustomGraphic;

    public override CellRect GetBoundaryRect() => section.CellRect;

    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);

        foreach (var pos in section.CellRect)
        {
            var roof = Map.roofGrid.RoofAt(pos);
            roof?.GetModExtension<RoofExtension>()?.customRoofGraphic?.DrawDataAt(Map, pos, roof)?.Print(this, pos);
        }

        FinalizeMesh(MeshParts.All);
    }
}