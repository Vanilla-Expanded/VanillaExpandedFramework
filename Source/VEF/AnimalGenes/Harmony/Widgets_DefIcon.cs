using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(Widgets))]
    [HarmonyPatch("DefIcon")]

    public class VEF_AnimalGenes_Widgets_DefIcon_Patch
    {
        [HarmonyPostfix]
        public static void DoGeneDefIcon(Rect rect, Def def, Color? color, float scale, Material material, float alpha)
        {
            AnimalGeneDef geneDef = def as AnimalGeneDef;
            if (geneDef != null)
            {
                GUI.color = color ?? geneDef.IconColor;
                CachedTexture cachedTexture = ITab_AnimalGenes.GetBackGround(geneDef.stability);
                GUI.DrawTexture(rect, cachedTexture.Texture);
                Widgets.DrawTextureFitted(rect, geneDef.Icon, scale, material, alpha);
                GUI.color = Color.white;
            }

            FeratypeDef feratypeDef = def as FeratypeDef;
            if (feratypeDef != null)
            {
                GUI.color = Color.white;
                Widgets.DrawTextureFitted(rect, feratypeDef.Icon, scale, material, alpha);
            }
        }
    }
}