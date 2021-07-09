using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VanillaFurnitureExpanded
{
   
    [HarmonyPatch(typeof(GhostUtility))]
    [HarmonyPatch("GhostGraphicFor")]
    public static class VanillaExpandedFramework_GhostUtility_GhostGraphicFor_Patch
    {
        [HarmonyPostfix]
        static void DisplayBlueprintGraphic(Graphic baseGraphic, ThingDef thingDef, Color ghostCol, ref Graphic __result)
        {

            if (thingDef.GetModExtension<ShowBlueprintExtension>() != null && thingDef.GetModExtension<ShowBlueprintExtension>().showBlueprintInGhostMode)
            {
                Graphic graphic = GraphicDatabase.Get(typeof(Graphic_Multi), thingDef.building.blueprintGraphicData.texPath, ShaderTypeDefOf.Cutout.Shader, baseGraphic.drawSize, Color.white, Color.white, thingDef.building.blueprintGraphicData, null);
                __result = graphic;
            }



        }
    }

}

