using System;
using System.Collections.Generic;
using System.Linq;
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

    public class CompProperties_VerbProps : CompProperties
    {
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
        public bool canBeToggled;
        public bool canFireIndependently;
        public DrawPosition defaultPosition;
        public string description;
        public bool draw;
        public GraphicData graphic;
        public string label;
        public Type managedClass;
        private Dictionary<string, DrawPosition> positions;
        public bool separateToggle;
        public List<DrawPosition> specificPositions;
        public string toggleDescription;
        public string toggleIconPath;
        public string toggleLabel;
        public string visualLabel;
        public Texture2D ToggleIcon { get; private set; }
        public Texture2D Icon { get; private set; }
        public Graphic Graphic { get; private set; }

        public IEnumerable<string> ConfigErrors()
        {
            if (label.NullOrEmpty()) yield return "label cannot be null or empty";

            if (!separateToggle && (!toggleLabel.NullOrEmpty() || !toggleDescription.NullOrEmpty() ||
                                    !toggleIconPath.NullOrEmpty()))
                yield return "don't provide toggle details without a separate toggle";

            if (!managedClass.IsSubclassOf(typeof(ManagedVerb)))
                yield return "managedClass must be subclass of ManagedVerb";

            if (canFireIndependently && !managedClass.IsSubclassOf(typeof(TurretVerb)))
                yield return "managedClass of independent verb must be a subclass of TurretVerb";
        }

        public Vector3 DrawPos(string name, Vector3 drawPos, Rot4 rot)
        {
            if (positions.TryGetValue(name, out var pos)) return drawPos + pos.ForRot(rot);

            pos = defaultPosition ?? DrawPosition.Zero;
            positions.Add(name, pos);
            return drawPos + pos.ForRot(rot);
        }

        public void Initialize()
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
                positions = new Dictionary<string, DrawPosition>();
                if (specificPositions != null)
                    foreach (var pos in specificPositions)
                        positions.Add(pos.defName, pos);
            }
        }
    }

    public class DrawPosition
    {
        private static readonly Vector2 PLACEHOLDER = Vector2.positiveInfinity;
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
}