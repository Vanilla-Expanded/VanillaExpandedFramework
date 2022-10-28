using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(GeneGraphicData), "GetGraphics")]
    public static class GeneGraphicData_GetGraphics_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(GeneGraphicData __instance, Pawn pawn, Shader skinShader, Color rottingColor, ref (Graphic, Graphic) __result)
        {
            Graphic item = GraphicDatabase.Get<Graphic_Multi>(__result.Item1.path, HasMaskTexture(__result.Item1.path) ? __result.Item1.Shader : ShaderDatabase.CutoutComplex, Vector2.one, __result.Item1.color, __result.Item1.colorTwo);
            Graphic item2 = GraphicDatabase.Get<Graphic_Multi>(__result.Item2.path, HasMaskTexture(__result.Item2.path) ? __result.Item2.Shader : ShaderDatabase.CutoutComplex, Vector2.one, __result.Item2.color, __result.Item2.colorTwo);
            __result = (item, item2);

            bool HasMaskTexture(string path)
            {
                if(ContentFinder<Texture2D>.Get(path + "_southm", reportFailure: false) != null)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
