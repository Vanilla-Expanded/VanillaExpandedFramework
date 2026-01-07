using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CompCustomizableGraphic : ThingComp
{
    public int? tempSelectedGraphicIndex;

    public CompProperties_CustomizableGraphic Props => props as CompProperties_CustomizableGraphic;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        if (!respawningAfterLoad && parent.overrideGraphicIndex == null)
        {
            var style = parent.StyleDef;
            if (style == null || !Props.defaultStyleIndex.TryGetValue(style, out var index))
                index = Props.defaultIndex;

            if (index >= 0)
                parent.overrideGraphicIndex = index;
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (parent.Spawned && parent is not Blueprint_Install)
        {
            var (def, style, color) = GetThingDefStyleDrawColor();

            // Only display the gizmo if the current style (if present)
            // or graphic (if no style) uses a supported graphic type.
            if (IsSupportedGraphicType(style?.Graphic ?? def.graphic))
            {
                var changeVisual = new Command_ActionSameThingAndStyleDef
                {
                    defaultLabel = Props.gizmoLabel ?? "VFE.ChangeVisualLabel".Translate(),
                    defaultDesc = Props.gizmoDescription ?? "VFE.ChangeVisualDesc".Translate(),
                    icon = Props.Icon,
                    targetDef = def,
                    targetStyleDef = style,
                    action = () => SelectGraphicMenu(def, style, color, Props),
                };

                yield return changeVisual;
            }
        }
    }

    private (ThingDef, ThingStyleDef, Color) GetThingDefStyleDrawColor()
    {
        // Not perfect for color, if the building is not stuffable it'll return any material color
        if (parent is Frame frame)
            return (frame.BuildDef, frame.StyleDef, frame.DrawColor);

        // Not perfect for color, if building is not stuffable 
        if (parent is Blueprint_Build blueprint)
            return (blueprint.BuildDef, blueprint.StyleDef, blueprint.stuffToUse == null ? Color.white : blueprint.BuildDef.GetColorForStuff(blueprint.stuffToUse));

        if (parent is Blueprint_Install install)
        {
            var thingToInstall = install.ThingToInstall;
            return (thingToInstall.def, thingToInstall.StyleDef, thingToInstall.DrawColor);
        }

        return (parent.def, parent.StyleDef, parent.DrawColor);
    }

    private static void SelectGraphicMenu(ThingDef def, ThingStyleDef style, Color color, CompProperties_CustomizableGraphic props)
    {
        int graphicCount;
        Func<int, Graphic> graphicGetter;

        switch (style?.Graphic ?? def.graphic)
        {
            case Graphic_Indexed indexed:
                graphicCount = indexed.SubGraphicsCount;
                graphicGetter = i => indexed.SubGraphicAtIndex(i);
                break;
            case Graphic_Random random:
                graphicCount = random.SubGraphicsCount;
                graphicGetter = i => random.SubGraphicAtIndex(i);
                break;
            default:
                return;
        }

        var list = new List<FloatMenuOption>();
        var comps = Find.Selector.SelectedObjects
            .OfType<ThingWithComps>()
            .Select(x => x.GetComp<CompCustomizableGraphic>())
            .Where(x =>
            {
                // Don't modify buildings with no comp or install blueprints
                if (x == null || x.parent is Blueprint_Install) return false;
                // Only modify buildings with matching def and style
                var (d, s, _) = x.GetThingDefStyleDrawColor();
                return d == def && s == style;
            })
            .ToList();
        if (style == null || props.styledGraphicData == null || !props.styledGraphicData.TryGetValue(style, out var data))
            data = props.defaultGraphicData;

        for (var i = 0; i < graphicCount; i++)
        {
            var index = i;
            var subGraphic = graphicGetter(i);
            string label;
            int sortingPriority;

            if (data != null && i < data.Count)
            {
                label = data[i].name;
                sortingPriority = data[i].sortingPriority;
            }
            else
            {
                label = subGraphic.path.Substring(subGraphic.path.LastIndexOf('/') + 1);
                sortingPriority = 0;
            }

            list.Add(new FloatMenuOption(
                label,
                () => SelectGraphic(comps, index),
                ContentFinder<Texture2D>.Get(subGraphic.path),
                color,
                mouseoverGuiAction: _ => SelectTemporaryGraphics(comps, index),
                orderInPriority: sortingPriority));
        }

        if (list.Any())
        {
            var menu = new FloatMenu(list)
            {
                onCloseCallback = () => SelectTemporaryGraphics(comps, null)
            };

            Find.WindowStack.Add(menu);
        }
        else
        {
            Log.Error($"Tried to select custom graphic for {def}, but no custom graphic found.");
        }
    }

    public static void SelectGraphic(List<CompCustomizableGraphic> comps, int graphicIndex)
    {
        foreach (var comp in comps)
            comp.SelectGraphic(graphicIndex);
    }

    public void SelectGraphic(int graphicIndex)
    {
        SelectGraphic(graphicIndex, false, IsSupportedGraphicType(parent.Graphic));
        // If the building has a blueprint, change its graphic as well
        var blueprint = InstallBlueprintUtility.ExistingBlueprintFor(parent);
        blueprint?.GetComp<CompCustomizableGraphic>()?.SelectGraphic(graphicIndex, false, IsSupportedGraphicType(blueprint.Graphic));
    }

    private static void SelectTemporaryGraphics(List<CompCustomizableGraphic> comps, int? graphicIndex)
    {
        foreach (var comp in comps)
        {
            if (comp.parent.Spawned)
                comp.SelectGraphic(graphicIndex, true, IsSupportedGraphicType(comp.parent.Graphic));
        }
    }

    private void SelectGraphic(int? graphicIndex, bool temporary, bool canChangeGraphic)
    {
        var sameIndex = graphicIndex == parent.OverrideGraphicIndex;

        // Lag prevention
        if (sameIndex && temporary)
            return;

        if (graphicIndex == null)
        {
            if (tempSelectedGraphicIndex != null)
                parent.overrideGraphicIndex = tempSelectedGraphicIndex;
        }
        else
        {
            if (temporary)
                tempSelectedGraphicIndex ??= parent.overrideGraphicIndex;
            else
                tempSelectedGraphicIndex = null;
            parent.overrideGraphicIndex = graphicIndex;
        }

        // Lag prevention, also don't change graphics if it's not possible (frame)
        if (!sameIndex && canChangeGraphic && parent.Spawned)
            parent.Map.mapDrawer.SectionAt(parent.Position).RegenerateAllLayers();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref tempSelectedGraphicIndex, nameof(tempSelectedGraphicIndex));

        if (Scribe.mode == LoadSaveMode.PostLoadInit && tempSelectedGraphicIndex != null)
        {
            parent.overrideGraphicIndex = tempSelectedGraphicIndex;
            tempSelectedGraphicIndex = null;
        }
    }

    private static bool IsSupportedGraphicType(Graphic graphic) => graphic is Graphic_Indexed or Graphic_Random;

    public void Rotate(RotationDirection direction)
    {
        if (direction == RotationDirection.None || !IsSupportedGraphicType(parent.Graphic))
            return;

        var index = parent.OverrideGraphicIndex ?? -1;
        if (index < 0)
            return;

        List<CompProperties_CustomizableGraphic.CustomizableGraphicOptionData> data;
        // Unstyled graphic, make sure default graphic data is not null
        if (parent.StyleDef == null)
        {
            if (Props.defaultGraphicData == null)
                return;

            data = Props.defaultGraphicData;
        }
        // Styled graphic, make sure we have styled graphic data for it (if any) and grab it
        else
        {
            if (Props.styledGraphicData == null || !Props.styledGraphicData.TryGetValue(parent.StyleDef, out data))
                return;
        }

        // Out of bounds check
        if (index >= data.Count)
            return;

        for (var i = 0; i < (int)direction; i++)
        {
            var entry = data[index];

            index = entry.clockwiseRotationIndex;
            // Out of bounds check
            if (index < 0 || index >= data.Count)
                break;
        }

        // Out of bounds check
        if (index >= 0 && index < data.Count)
            SelectGraphic(index);
    }

    private class Command_ActionSameThingAndStyleDef : Command_Action
    {
        public ThingDef targetDef;
        public ThingStyleDef targetStyleDef;

        public override bool GroupsWith(Gizmo other)
            => other is Command_ActionSameThingAndStyleDef command && command.targetDef == targetDef && command.targetStyleDef == targetStyleDef && base.GroupsWith(other);
    }
}