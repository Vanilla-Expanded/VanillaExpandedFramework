using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Comps
{
    public class HediffCompProperties_ExtendedVerbGiver : HediffCompProperties_VerbGiver
    {
        public List<AdditionalVerbProps> verbProps;

        public HediffCompProperties_ExtendedVerbGiver()
        {
            compClass = typeof(HediffComp_ExtendedVerbGiver);
        }

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                foreach (var props in verbProps) props.Initialize();
            });
        }
    }

    public class CompProperties_VerbGiver : CompProperties_VerbProps
    {
        public CompProperties_VerbGiver()
        {
            compClass = typeof(Comp_VerbGiver);
        }
    }

    public class Comp_VerbProps : ThingComp
    {
        public CompProperties_VerbProps Props => props as CompProperties_VerbProps;
    }

    public class CompProperties_VerbProps : CompProperties
    {
        public bool ConsiderMelee;
        public List<AdditionalVerbProps> verbProps;

        public override void PostLoadSpecial(ThingDef parent)
        {
            base.PostLoadSpecial(parent);
            if (verbProps != null)
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    foreach (var props in verbProps) props.Initialize();
                });
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef)) yield return error;

            foreach (var error in verbProps.SelectMany(prop => prop.ConfigErrors()))
                yield return error;
        }

        public AdditionalVerbProps PropsFor(Verb verb)
        {
            var label = verb.verbProps.label;
            return string.IsNullOrEmpty(label) ? null : verbProps?.FirstOrDefault(prop => prop.label == label);
        }
    }

    public class AdditionalVerbProps
    {
        public static BodyTypeDef NA = new BodyTypeDef();
        public bool canBeToggled;
        public bool canFireIndependently;
        public bool colonistOnly;
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
        }

        public virtual Vector3 DrawPos(Pawn pawn, Vector3 drawPos, Rot4 rot)
        {
            DrawPosition pos = null;
            if (positions.TryGetValue(pawn.def.defName, out var dic) ||
                humanAsDefault && positions.TryGetValue(ThingDefOf.Human.defName, out dic))
                if (!(pawn.story?.bodyType != null && dic.TryGetValue(pawn.story.bodyType, out pos)))
                    dic.TryGetValue(NA, out pos);

            pos = pos ?? defaultPosition ?? DrawPosition.Zero;
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

        public virtual void Initialize()
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
        }
    }

    public class DrawPosition
    {
        private static readonly Vector2 PLACEHOLDER = Vector2.positiveInfinity;
        public BodyTypeDef BodyType = AdditionalVerbProps.NA;
        public Vector2 Default = PLACEHOLDER;
        public string defName;
        public Vector2 Down = PLACEHOLDER;
        public Vector2 Left = PLACEHOLDER;
        public Vector2 Right = PLACEHOLDER;
        public Vector2 Up = PLACEHOLDER;

        public static DrawPosition Zero => new DrawPosition
        {
            defName = "",
            Default = Vector2.zero
        };

        public Vector3 ForRot(Rot4 rot)
        {
            var vec = PLACEHOLDER;
            switch (rot.AsInt)
            {
                case 0:
                    vec = Up;
                    break;
                case 1:
                    vec = Right;
                    break;
                case 2:
                    vec = Down;
                    break;
                case 3:
                    vec = Left;
                    break;
                default:
                    vec = Default;
                    break;
            }

            if (double.IsPositiveInfinity(vec.x)) vec = Default;
            if (double.IsPositiveInfinity(vec.x)) vec = Vector2.zero;
            return new Vector3(vec.x, 0, vec.y);
        }
    }

    public class Scaling
    {
        public BodyTypeDef BodyType;
        public string defName;
        public float scaling;
    }
}