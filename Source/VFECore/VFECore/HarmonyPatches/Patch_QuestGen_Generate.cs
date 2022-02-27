using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(QuestGen), "Generate")]
    public static class Patch_QuestGen_Generate
    {
        static int iterationNumber = 0;

        public static void Postfix(QuestScriptDef root, Slate initialVars, ref Quest __result)
        {
            if (RealInvolvedFactions(__result).Any(f => f.def.HasModExtension<ExcludeFromQuestsExtension>()))
            {
                // Log.Message($"QuestGen Generate - {__result.name} contains not allowed faction(s) - Regenerating quest...");
                Quest newResult = null;

                while (newResult == null || RealInvolvedFactions(newResult).Any(f => f.def.HasModExtension<ExcludeFromQuestsExtension>()))
                {
                    newResult = QuestGen.Generate(root, initialVars);
                    iterationNumber++;
                }

                __result = newResult;
            }
            if (iterationNumber > 0)
            {
                // Log.Message($"QuestGen Generate - Regenerated quest {iterationNumber} time - Regeneration ended...");
                iterationNumber = 0;
            }
        }

        private static List<Faction> RealInvolvedFactions(Quest quest)
        {
            List<Faction> factions = new List<Faction>();

            if (quest != null && quest.PartsListForReading != null)
            {
                foreach (QuestPart part in quest.PartsListForReading)
                {
                    if (part != null && part.InvolvedFactions != null)
                    {
                        foreach (Faction faction in part.InvolvedFactions)
                        {
                            if (!factions.Contains(faction))
                            {
                                factions.Add(faction);
                            }
                        }
                    }
                    if (part is QuestPart_SpawnWorldObject qpswo && qpswo != null && qpswo.worldObject?.Faction != null)
                    {
                        if (!factions.Contains(qpswo.worldObject.Faction))
                        {
                            factions.Add(qpswo.worldObject.Faction);
                        }
                    }
                }
            }
            return factions;
        }
    }
}