using HarmonyLib;
using Verse;

namespace VEF.AnimalBehaviours;

[HarmonyPatch(typeof(PlayDataLoader), nameof(PlayDataLoader.LoadAllPlayData))]
public class VanillaExpandedFramework_PlayDataLoader_LoadAllPlayData_Patch
{
    private static void Postfix()
    {
        // If the language is ever changed, the cached text would change, and the logic would break.
        // Update the text if the translation ever changes.
        AnimalBehavioursPawnGizmos.toggleCache = "CommandToggleDraftDesc".Translate().ToString();
    }
}