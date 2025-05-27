using UnityEngine;
using Verse;

namespace VFECore;

public class Graphic_DarklightSingle : Graphic_Single
{
    // Graphic_DarklightSingle and Graphic_DarklightMulti use a separate graphic for normal and darklight graphic.
    // The darklight graphics will be loaded using the same name as normal graphics, but with "_dark" suffix.
    // In case of Graphic_DarklightMulti, the suffix comes before rotation variants, for example "_dark_east".
    // Thay also both share the masked graphic. However, due to the way code works, for Graphic_DarklightMulti the file
    // name is slightly different, with "_m" rather than just "m" suffix, and it comes before rotation like "_m_east".
    // 
    // They both require a glower to dirty map mesh over its occupied area.
    // We include CompGlower_DirtyMapMesh to do just that, but any glower that dirties map mesh will work.

    public Graphic darklightGraphic;

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        if (GlowerUtility.IsDarklight(thing))
            return darklightGraphic.MatAt(rot, thing);
        return base.MatAt(rot, thing);
    }

    public override Material MatSingleFor(Thing thing)
    {
        if (GlowerUtility.IsDarklight(thing))
            return darklightGraphic.MatSingleFor(thing);
        return base.MatSingleFor(thing);
    }

    public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
    {
        base.TryInsertIntoAtlas(groupKey);
        darklightGraphic.TryInsertIntoAtlas(groupKey);
    }

    public override void Init(GraphicRequest req)
    {
        req.maskPath ??= req.path + MaskSuffix;
        base.Init(req);
        req.path += "_dark";

        darklightGraphic = new Graphic_Single();
        darklightGraphic.Init(req);
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        => GraphicDatabase.Get<Graphic_DarklightSingle>(path, newShader, drawSize, newColor, newColorTwo, data);

    public override string ToString() => $"{nameof(Graphic_DarklightSingle)}(base=({base.ToString()}), darklight=({darklightGraphic}))";
}