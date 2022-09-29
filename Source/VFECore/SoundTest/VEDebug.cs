using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public class TextureButton
    {
        public static Texture2D VFELogo = ContentFinder<Texture2D>.Get("UI/Widgets/VFELogo");
    }

    [DefOf]
    public class Restart
    {
        public static KeyBindingDef VFE_Dev_Restart;
    }

    [StaticConstructorOnStartup]
    public static class VEDebug
    {
        // Non public field
        public static FieldInfo UIRoot_DebugWindowsOpener = AccessTools.Field(typeof(UIRoot), "debugWindowOpener");
        public static FieldInfo DebugWindowsOpener_widgetRow = AccessTools.Field(typeof(DebugWindowsOpener), "widgetRow");

        private static void AddVEOptions()
        {
            var opener = (DebugWindowsOpener)UIRoot_DebugWindowsOpener.GetValue(Find.UIRoot);
            var widgetRow = (WidgetRow)DebugWindowsOpener_widgetRow.GetValue(opener);

            if (widgetRow.ButtonIcon(TextureButton.VFELogo, "More options.."))
            {
                var floatMenuOptions = new List<FloatMenuOption>();
                // Sound test, only on map
                if (Current.ProgramState == ProgramState.Playing)
                {
                    floatMenuOptions.Add(new FloatMenuOption("Sound test", () =>
                    {
                        if (!Find.WindowStack.TryRemove(typeof(EditWindow_SoundTest)))
                            Find.WindowStack.Add(new EditWindow_SoundTest());
                    }, MenuOptionPriority.High));
                }
                // Restart
                floatMenuOptions.Add(new FloatMenuOption("Restart", () =>
                {
                    GenCommandLine.Restart();
                }));
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
            }
        }
    }
}
