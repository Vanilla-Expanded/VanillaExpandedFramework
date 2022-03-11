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
        public string label;
        public Type managedClass;
        private Dictionary<string, Dictionary<BodyTypeDef, DrawPosition>> positions;
        private Dictionary<string, Dictionary<BodyTypeDef, Scaling>> scale;
        public List<Scaling> scalings;
        public bool separateToggle;
        public List<DrawPosition> specificPositions;
        public string toggleDescription;
        public string toggleIconPath;
        public string toggleLabel;
        public bool uniqueTargets;
        public string visualLabel;
        public Texture2D ToggleIcon { get; protected set; }
        public Texture2D Icon { get; protected set; }
        public Graphic Graphic { get; protected set; }

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

            if (managedClass != null && canFireIndependently && !managedClass.IsSubclassOf(typeof(TurretVerb)))
                yield return "managedClass of independent verb must be a subclass of TurretVerb";

            if (managedClass != null && draw && !managedClass.IsSubclassOf(typeof(DrawnVerb)))
                yield return "managedClass of drawn verb must be a subclass of DrawnVerb";
        }

        public void ResolveReferences()
        {
            foreach (var comp in comps) comp.ResolveReferences();
        }

        public virtual Vector3 DrawPos(Pawn pawn, Vector3 drawPos, Rot4 rot)
        {
            DrawPosition pos = null;
            if (positions.TryGetValue(pawn.def.defName, out var dic) ||
                humanAsDefault && positions.TryGetValue(ThingDefOf.Human.defName, out dic))
                if (!(pawn.story?.bodyType != null && dic.TryGetValue(pawn.story.bodyType, out pos)))
                    dic.TryGetValue(NA, out pos);

            pos ??= defaultPosition ?? DrawPosition.Zero;
            return drawPos + pos.ForRot(rot);
        }

        public virtual float Scale(Pawn pawn)
        {
            Scaling s = null;
            if (scale.TryGetValue(pawn.def.defName, out var dic) ||
                humanAsDefault && scale.TryGetValue(ThingDefOf.Human.defName, out dic))
                if (!(pawn.story?.bodyType != null && dic.TryGetValue(pawn.story.bodyType, out s)))
                    dic.TryGetValue(NA, out s);

            return s?.scaling ?? (drawScale == 0 ? 1f : drawScale);
        }

        public virtual void Initialize(VerbProperties parent)
        {
            if (!string.IsNullOrWhiteSpace(toggleIconPath))
                ToggleIcon = ContentFinder<Texture2D>.Get(toggleIconPath);
            if (graphic != null)
            {
                Graphic = graphic.Graphic;
                Icon = (Texture2D) Graphic.ExtractInnerGraphicFor(null).MatNorth.mainTexture;
            }

            if (positions == null)
            {
                positions = new Dictionary<string, Dictionary<BodyTypeDef, DrawPosition>>();
                if (specificPositions != null)
                    foreach (var pos in specificPositions)
                        if (!positions.ContainsKey(pos.defName))
                            positions.Add(pos.defName, new Dictionary<BodyTypeDef, DrawPosition> {{pos.BodyType, pos}});
                        else
                            positions[pos.defName].Add(pos.BodyType, pos);
            }

            if (scale == null)
            {
                scale = new Dictionary<string, Dictionary<BodyTypeDef, Scaling>>();
                if (scalings != null)
                    foreach (var scaling in scalings)
                        if (scale.ContainsKey(scaling.defName))
                            scale[scaling.defName].Add(scaling.BodyType, scaling);
                        else
                            scale.Add(scaling.defName,
                                new Dictionary<BodyTypeDef, Scaling> {{scaling.BodyType, scaling}});
            }

            if (comps.Any())
                foreach (var comp in comps)
                    comp.PostLoad(parent, this);
        }
    }
}