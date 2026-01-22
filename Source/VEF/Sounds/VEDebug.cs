using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Sounds
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
        public static readonly AccessTools.FieldRef<DebugWindowsOpener, WidgetRow> DebugWindowsOpener_widgetRow = AccessTools.FieldRefAccess<DebugWindowsOpener, WidgetRow>("widgetRow");

        private static void AddVEOptions()
        {
            var opener = Find.UIRoot.debugWindowOpener;
            var widgetRow = DebugWindowsOpener_widgetRow(opener);

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
