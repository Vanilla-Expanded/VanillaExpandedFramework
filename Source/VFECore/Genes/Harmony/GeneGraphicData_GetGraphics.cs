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
            Graphic item1 = __result.Item1;
            Graphic item2 = __result.Item2;
            if (HasMaskTexture(item1.path))
            {
                item1 = GraphicDatabase.Get<Graphic_Multi>(item1.path, ShaderDatabase.CutoutComplex, Vector2.one, item1.color, item1.colorTwo);
            }
            if (HasMaskTexture(item2.path))
            {
                item2 = GraphicDatabase.Get<Graphic_Multi>(item2.path, ShaderDatabase.CutoutComplex, Vector2.one, item2.color, item2.colorTwo);
            }
            __result = (item1, item2);

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
