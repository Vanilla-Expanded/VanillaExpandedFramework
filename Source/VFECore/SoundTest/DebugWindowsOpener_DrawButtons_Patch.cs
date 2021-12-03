using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore.SoundTest
{
    [HarmonyPatch]
    public static class DebugWindowsOpener_DrawButtons_Patch
    {
        [HarmonyPatch(typeof(DebugWindowsOpener), "DrawButtons")]
        [HarmonyPostfix]
        public static void Postfix(WidgetRow ___widgetRow)
        {
            if (___widgetRow.ButtonIcon(TextureButton.SoundNote, "Open sound tester."))
            {
                if (!Find.WindowStack.TryRemove(typeof(EditWindow_SoundTest)))
                    Find.WindowStack.Add(new EditWindow_SoundTest());
            }
        }
    }
}
