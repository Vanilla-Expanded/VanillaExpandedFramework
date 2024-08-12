using HarmonyLib;
using System.Linq;
using UnityEngine;
using Verse;

namespace GraphicCustomization
{
    public class GraphicCustomizationMod : Mod
    {        
        public GraphicCustomizationMod(ModContentPack pack) : base(pack)
        {
            new Harmony("GraphicCustomization.Mod").PatchAll();
        }
    }
}
