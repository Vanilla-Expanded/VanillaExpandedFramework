using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace VEF.Storyteller
{
    [HarmonyPatch(typeof(QuestManager), "Add")]
    public static class VanillaExpandedFramework_QuestManager_Add_Patch
    {
        public static void Postfix(Quest quest)
        {
            if (quest?.root != null)
            {
                var extension = quest.root.GetModExtension<QuestChainExtension>();
                if (extension != null)
                {
                    GameComponent_QuestChains.Instance.quests ??= new List<QuestInfo>();
                    GameComponent_QuestChains.Instance.quests.Add(new QuestInfo
                    {
                        questRef = quest,
                        questDef = quest.root,
                    });
                }
            }
        }
    }
}
