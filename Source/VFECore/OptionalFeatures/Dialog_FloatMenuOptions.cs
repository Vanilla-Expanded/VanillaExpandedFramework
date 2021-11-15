using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore.OptionalFeatures
{
    public class Dialog_FloatMenuOptions : Window
    {
        private readonly List<FloatMenuOption> options;
        private readonly Dictionary<FloatMenuOption, ThingDef> shownItems;
        private Vector2 scrollPosition = new Vector2(0, 0);
        private string searchText = "";

        public Dialog_FloatMenuOptions(List<FloatMenuOption> opts)
        {
            options = opts;
            var info1 = AccessTools.Field(typeof(FloatMenuOption), "shownItem");
            shownItems = opts.ToDictionary(opt => opt, opt => (ThingDef) info1.GetValue(opt));
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(620f, 500f);

        public static void ApplyFeature(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(IdeoUIUtility), "DoPreceptsInt"), transpiler: new HarmonyMethod(typeof(Dialog_FloatMenuOptions), nameof(Transpiler)));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Method(typeof(WindowStack), nameof(WindowStack.Add));
            var idx1 = list.FindIndex(ins => ins.Calls(info1));
            var label1 = generator.DefineLabel();
            var label2 = generator.DefineLabel();
            list[idx1].labels.Add(label1);
            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Br, label1),
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Dialog_FloatMenuOptions), new[] {typeof(List<FloatMenuOption>)})).WithLabels(label2)
            });
            list.InsertRange(idx1 - 1, new[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(List<FloatMenuOption>), "Count")),
                new CodeInstruction(OpCodes.Ldc_I4, 30),
                new CodeInstruction(OpCodes.Bge, label2)
            });
            return list;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            var outRect = new Rect(inRect);
            outRect.yMin += 20f;
            outRect.yMax -= 40f;
            outRect.width -= 16f;
            searchText = Widgets.TextField(outRect.TopPartPixels(35f), searchText);
            outRect.yMin += 40f;
            var shownOptions = options.Where(opt => opt.Label.ToLower().Contains(searchText.ToLower())).ToList();
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, shownOptions.Sum(opt => opt.RequiredHeight + 17f));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                var y = 0f;
                foreach (var opt in shownOptions)
                {
                    var height = opt.RequiredHeight + 10f;
                    var rect2 = new Rect(0f, y, viewRect.width - 7f, height);
                    if (shownItems[opt] != null)
                    {
                        rect2.xMax -= Widgets.InfoCardButtonSize + 7f;
                        Widgets.InfoCardButton(rect2.xMax + 7f, rect2.y + 1f, shownItems[opt]);
                    }

                    if (opt.DoGUI(rect2, false, null))
                    {
                        Close();
                        break;
                    }

                    GUI.color = Color.white;
                    y += height + 7f;
                }
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }
    }
}