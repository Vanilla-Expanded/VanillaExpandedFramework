using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.VerbComps;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Comps
{
    public class AdditionalVerbProps
    {
        public static BodyTypeDef NA = new();
        public float additionalCooldownTime;
        public bool brawlerCaresAbout = true;
        public bool canBeToggled = true;
        public bool canFireIndependently;
        public bool colonistOnly;
        public List<VerbCompProperties> comps = new();
        public DrawPosition defaultPosition;
        public string description;
        public bool draw;
        public float drawScale = 1f;
        public GraphicData graphic;
        public bool humanAsDefault;
        public Texture2D Icon;
        public string label;
        public Type managedClass;
        public List<Scaling> scalings;
        public bool separateToggle;
        public List<DrawPosition> specificPositions;
        public string toggleDescription;
        public string toggleIconPath;
        public string toggleLabel;
        public bool uniqueTargets;
        public string visualLabel;
        public Texture2D ToggleIcon { get; private set; }

        public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in ConfigErrors()) yield return error;

            var matches = parentDef.Verbs.Count(vb => vb.label == label);
            if (matches == 0) yield return $"Could not find verb on parent with label \"{label}\"";
            if (matches > 1)
                yield return $"Found too many verbs on parent with label \"{label}\". Expected 1, found {matches}";

            if (parentDef.Verbs.FirstOrDefault(vb => vb.label == label) is { } props)
                foreach (var comp in comps)
                foreach (var error in comp.ConfigErrors(props, this))
                    yield return error;
        }

        public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var error in ConfigErrors()) yield return error;
            if (parentDef.comps.OfType<HediffCompProperties_VerbGiver>().FirstOrDefault() is { } verbGiver)
            {
                var matches = verbGiver.verbs.Count(vb => vb.label == label);
                if (matches == 0) yield return $"Could not find verb on parent with label \"{label}\"";
                if (matches > 1)
                    yield return $"Found too many verbs on parent with label \"{label}\". Expected 1, found {matches}";

                if (verbGiver.verbs.FirstOrDefault(vb => vb.label == label) is { } props)
                    foreach (var comp in comps)
                    foreach (var error in comp.ConfigErrors(props, this))
                        yield return error;
            }
            else
                yield return "Could not find HediffCompProperties_VerbGiver, meaning this has VerbProps and no verbs.";
        }

        public virtual IEnumerable<string> ConfigErrors()
        {
            if (label.NullOrEmpty()) yield return "label cannot be null or empty";

            if (!separateToggle && (!toggleLabel.NullOrEmpty() || !toggleDescription.NullOrEmpty() ||
                                    !toggleIconPath.NullOrEmpty()))
                yield return "don't provide toggle details without a separate toggle";

            if (managedClass != null && !managedClass.IsSubclassOf(typeof(ManagedVerb)))
                yield return "managedClass must be subclass of ManagedVerb";

            if (managedClass != null && comps.Any() && !managedClass.IsSubclassOf(typeof(VerbWithComps)))
                yield return "Has comps but is not VerbWithComps";

            if (draw) yield return "draw is deprecated, use VerbCompProperties_Draw";
            if (canFireIndependently) yield return "canFireIndependently is deprecated, use VerbCompProperties_Turret";
        }

        public void ResolveReferences()
        {
            foreach (var comp in comps) comp.ResolveReferences();
        }

        public virtual void Initialize(VerbProperties parent)
        {
            if (!string.IsNullOrWhiteSpace(toggleIconPath))
                ToggleIcon = ContentFinder<Texture2D>.Get(toggleIconPath);

            if (canFireIndependently)
                comps.Add(new VerbCompProperties_Turret
                {
                    compClass = typeof(VerbComp_Turret),
                    defaultPosition = defaultPosition,
                    drawScale = drawScale,
                    graphic = graphic,
                    humanAsDefault = humanAsDefault,
                    scalings = scalings,
                    specificPositions = specificPositions,
                    uniqueTargets = uniqueTargets,
                    invisible = !draw
                });
            else if (draw)
                comps.Add(new VerbCompProperties_Draw
                {
                    compClass = typeof(VerbComp_Draw),
                    defaultPosition = defaultPosition,
                    drawScale = drawScale,
                    graphic = graphic,
                    humanAsDefault = humanAsDefault,
                    scalings = scalings,
                    specificPositions = specificPositions
                });

            if (comps.Any())
                foreach (var comp in comps)
                    comp.PostLoadSpecial(parent, this);
        }
    }
}