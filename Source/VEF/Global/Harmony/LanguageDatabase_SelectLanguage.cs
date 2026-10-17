using HarmonyLib;
using VEF.AnimalBehaviours;
using Verse;

namespace VEF;

[HarmonyPatch(typeof(LanguageDatabase), nameof(LanguageDatabase.SelectLanguage))]
public class VanillaExpandedFramework_LanguageDatabase_SelectLanguage_Patch
{
    private static void Postfix()
    {
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            // If the language is ever changed, the cached text would change, and the logic would break.
            // Update the text if the translation ever changes.
            AnimalBehavioursPawnGizmos.toggleCache = "CommandToggleDraftDesc".Translate().ToString();
        });
    }
}