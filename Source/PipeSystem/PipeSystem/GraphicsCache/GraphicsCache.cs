using System;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {
        public static readonly Material InputCellMaterial = MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent, new Color(0.686f, 0.471f, 0.255f));
        public static readonly Material OutputCellMaterial = MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent, new Color(0.357f, 0.686f, 0.369f));

    }
}
