using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_Pawn_GetInspectString_Patch
    {

        public static void AddInspectString(Pawn __instance, ref string __result)
        {

            if (__instance.equipment?.Primary != null)
            {
                var comp = __instance.equipment.Primary.TryGetComp<CompApplyWeaponTraits>();
                if (comp?.cachedLimitedUses>0)
                {
                    var compInspectString = comp.ShotRemainingInfo();
                    var lines = __result.Split('\n').ToList();
                    var equippedLineIndex = -1;
                    for (var i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].StartsWith("Equipped".TranslateSimple() + ": "))
                        {
                            equippedLineIndex = i;
                            break;
                        }
                    }

                    if (equippedLineIndex != -1)
                    {
                        lines.Insert(equippedLineIndex + 1, compInspectString);
                        __result = string.Join("\n", lines);
                    }
                    else
                    {
                        __result += "\n" + compInspectString;
                    }
                }
            }


        }
    }


}
