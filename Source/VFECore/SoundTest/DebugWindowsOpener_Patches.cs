using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(DebugWindowsOpener))]
    [HarmonyPatch("DrawButtons", MethodType.Normal)]
    public class DebugWindowsOpener_DrawButtons_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var count = codes.Count;

            for (int i = 0; i < count; i++)
            {
                if (i > 1 && codes[i - 1].opcode == OpCodes.Ldloc_2 && codes[i].opcode == OpCodes.Call)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VEDebug), "AddVEOptions"));
                }
                else
                {
                    yield return codes[i];
                }
            }
            yield break;
        }
    }

    [HarmonyPatch(typeof(DebugWindowsOpener))]
    [HarmonyPatch("DevToolStarterOnGUI", MethodType.Normal)]
    public class DebugWindowsOpener_DevToolStarterOnGUI_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (Restart.VFE_Dev_Restart.KeyDownEvent)
            {
                GenCommandLine.Restart();
            }
        }
    }
}