using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
using Verse;
using System.Reflection.Emit;
using System.Reflection;

namespace ModSettingsFramework
{
    [HarmonyPatch(typeof(Page_ModsConfig), "DoModInfo")]
    public static class Page_ModsConfig_DoModInfo_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var addCustomOptionsMethod = AccessTools.Method(typeof(Page_ModsConfig_DoModInfo_Transpiler), nameof(AddCustomMenuOptions));
            var codes = new List<CodeInstruction>(instructions);
            var nestedType = typeof(Page_ModsConfig).GetNestedType("<>c__DisplayClass71_0", BindingFlags.NonPublic);
            var modField = AccessTools.Field(nestedType, "mod");

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (i > 15 && codes[i - 15].OperandIs("ModFolder") && codes[i].opcode == OpCodes.Callvirt
                    && codes[i].operand is MethodInfo addMethod && addMethod.Name == "Add")
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, modField);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, addCustomOptionsMethod);
                }
            }
        }

        public static void AddCustomMenuOptions(List<FloatMenuOption> list, ModMetaData mod, Page_ModsConfig __instance)
        {
            if (mod != null)
            {
                var modHandles = new List<ModSettingsFrameworkMod>();
                foreach (var modSettings in ModSettingsFrameworkSettings.modSettingsPerModId)
                {
                    if (modSettings.Value.modHandle != null
                        && modSettings.Value.modHandle.modPackOverride.PackageIdPlayerFacing == mod.PackageIdPlayerFacing)
                    {
                        modHandles.Add(modSettings.Value.modHandle);
                    }
                }
                if (modHandles.Count > 0)
                {
                    if (modHandles.Count > 1 || (__instance.primaryModHandle != null && !__instance.primaryModHandle.SettingsCategory().NullOrEmpty()))
                    {
                        foreach (var modHandle in modHandles)
                        {
                            list.Add(new FloatMenuOption("ModOptions".Translate() + ": " + modHandle.SettingsCategory(), () =>
                            {
                                Find.WindowStack.Add(new Dialog_ModSettings(modHandle));
                            }));
                        }
                    }
                    else
                    {
                        list.Add(new FloatMenuOption("ModOptions".Translate(), () =>
                        {
                            Find.WindowStack.Add(new Dialog_ModSettings(modHandles[0]));
                        }));
                    }
                }
            }
        }
    }
}
