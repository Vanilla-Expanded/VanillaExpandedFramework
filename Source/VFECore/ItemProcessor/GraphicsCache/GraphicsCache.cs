using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Collections;

namespace ItemProcessor
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {

        //This class holds cached graphics so they can be accessed by the Building_ItemProcessor class

        //Colours for fermenter machines
        public static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);
        public static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

        //Colours for factory machines
        public static readonly Color FactoryBarZeroProgressColor = new Color(0.9f,0.04f,0.02f);
        public static readonly Color FactoryBarFinishedColor = new Color(0.02f, 0.46f, 0f);

        public static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);

        //Pretty interaction cells for automatic machines
        public static readonly Material InputCellMaterial = MaterialPool.MatFrom("UI/Overlays/IP_InputSlotOverlay", ShaderDatabase.Cutout);
        public static readonly Material OutputCellMaterial = MaterialPool.MatFrom("UI/Overlays/IP_OutputSlotOverlay", ShaderDatabase.Cutout);

    }
}

