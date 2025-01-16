using HarmonyLib;
using RimWorld;

namespace VFECore
{
    [HarmonyPatch(typeof(QuestManager), "Add")]
    public static class QuestManager_Add_Patch
    {
        public static void Postfix(Quest quest)
        {
            var extension = quest.root.GetModExtension<QuestChainExtension>();
            if (extension != null)
            {
                GameComponent_QuestChains.Instance.quests.Add(new QuestInfo
                {
                    quest = quest,
                    questDef = quest.root,
                });
            }
        }
    }
}