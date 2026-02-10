using System;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {
        public static readonly Material InputCellMaterial = MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent,Color.yellow);
        public static readonly Material OutputCellMaterial = MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent, Color.green);

    }
}
