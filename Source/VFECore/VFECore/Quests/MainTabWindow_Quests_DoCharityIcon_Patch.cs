using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(MainTabWindow_Quests), "DoCharityIcon")]
    public static class MainTabWindow_Quests_DoCharityIcon_Patch
    {
        public static void Postfix(MainTabWindow_Quests __instance, Rect innerRect, Quest ___selected)
        {
            if (___selected != null)
            {
                var extension = ___selected.root.GetModExtension<QuestChainExtension>();
                if (extension != null)
                {
                    bool isCharity = ___selected.charity && ModsConfig.IdeologyActive;
                    Rect rect = new Rect(innerRect.xMax - 32f - 26f - 32f - 4f, innerRect.y, 32f, 32f);
                    if (isCharity)
                    {
                        rect.x -= 32f + 4f;
                    }
                    GUI.DrawTexture(rect, extension.questChainDef.icon);
                    if (Mouse.IsOver(rect))
                    {
                        TooltipHandler.TipRegion(rect, "VEF.QuestChainTooltip".Translate(extension.questChainDef.label, extension.questChainDef.Worker.GetDescription()));
                    }
                }

            }
        }
    }
}