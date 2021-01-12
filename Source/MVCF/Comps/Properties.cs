using System.Collections.Generic;
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
    }

    public class CompProperties_VerbGiver : CompProperties
    {
        public List<AdditionalVerbProps> verbProps;

        public CompProperties_VerbGiver()
        {
            compClass = typeof(Comp_VerbGiver);
        }
    }

    public class AdditionalVerbProps
    {
        public bool canBeToggled;
        public bool canFireIndependently;
        public DrawPosition defaultPosition;
        public bool draw;
        public GraphicData graphic;
        public string label;
        private Dictionary<string, DrawPosition> positions;
        public List<DrawPosition> specificPositions;
        public string toggleIconPath;
        public string toggleLabel;

        public Texture2D ToggleIcon { get; private set; }
        public Graphic Graphic { get; private set; }

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
            if (graphic != null) Graphic = graphic.Graphic;
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