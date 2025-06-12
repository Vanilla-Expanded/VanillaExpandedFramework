﻿using Verse;

namespace VEF.Graphics;

public class CompGlower_DirtyMapMesh : CompGlower
{
    protected override void SetGlowColorInternal(ColorInt? color)
    {
        base.SetGlowColorInternal(color);

        // Required by Graphic_DarklightMulti and Graphic_DarklightSingle
        if (parent.Spawned)
            parent.DirtyMapMesh(parent.Map);

        // We technically make a patch to CompGlower.SetGlowColorInternal
        // (and check if Graphic requires dirtying), but I'd rather avoid
        // that due to other mods having the possibility to do stuff like calling
        // that method every single tick, making our (theoretical) patch
        // affect performance more than necessary.
    }
}