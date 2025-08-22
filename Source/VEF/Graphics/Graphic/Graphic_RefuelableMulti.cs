using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Graphics;

public class Graphic_RefuelableMulti : Graphic_Multi
{
    public Graphic emptyGraphic;

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        if (thing.TryGetComp<CompRefuelable>()?.Fuel <= 0)
            return emptyGraphic.MatAt(rot, thing);
        return base.MatAt(rot, thing);
    }

    public override Material MatSingleFor(Thing thing)
    {
        if (thing.TryGetComp<CompRefuelable>()?.Fuel <= 0)
            return emptyGraphic.MatSingleFor(thing);
        return base.MatSingleFor(thing);
    }

    public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
    {
        base.TryInsertIntoAtlas(groupKey);
        emptyGraphic.TryInsertIntoAtlas(groupKey);
    }

    public override void Init(GraphicRequest req)
    {
        req.maskPath ??= req.path + Graphic_Single.MaskSuffix;
        base.Init(req);
        req.path += "_empty";

        emptyGraphic = new Graphic_Multi();
        emptyGraphic.Init(req);
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        => GraphicDatabase.Get<Graphic_RefuelableSingle>(path, newShader, drawSize, newColor, newColorTwo, data);

    public override string ToString() => $"{nameof(Graphic_RefuelableSingle)}(base=({base.ToString()}), empty=({emptyGraphic}))";
}