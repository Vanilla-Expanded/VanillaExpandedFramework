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
        private HediffDef parent;
        public List<AdditionalVerbProps> verbProps;

        public HediffCompProperties_ExtendedVerbGiver() => compClass = typeof(HediffComp_ExtendedVerbGiver);

        public override void PostLoad()
        {
            base.PostLoad();
            if (verbProps != null)
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    foreach (var props in verbProps)
                        props?.Initialize(parent.comps.OfType<HediffCompProperties_VerbGiver>().FirstOrDefault()?.verbs.FirstOrDefault(v => v.label == props.label));
                });
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef)) yield return error;

            if (verbProps == null)
                yield return "No verbProps provided!";
            else
                foreach (var error in verbProps.SelectMany(prop => prop.ConfigErrors(parentDef)))
                    yield return error;
        }

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            this.parent = parent;
            foreach (var verbProp in verbProps) verbProp.ResolveReferences();
        }
    }

    public class CompProperties_VerbGiver : CompProperties_VerbProps
    {
        public CompProperties_VerbGiver() => compClass = typeof(Comp_VerbGiver);
    }

    public class Comp_VerbProps : ThingComp
    {
        public CompProperties_VerbProps Props => props as CompProperties_VerbProps;

        public AdditionalVerbProps PropsFor(Verb verb) => Props.PropsFor(verb);
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
                    foreach (var props in verbProps) props?.Initialize(parent.Verbs.FirstOrDefault(v => v.label == props.label));
                });
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef)) yield return error;

            if (verbProps == null)
                yield return "No verbProps provided!";
            else
                foreach (var error in verbProps.SelectMany(prop => prop.ConfigErrors(parentDef)))
                    yield return error;
        }

        public AdditionalVerbProps PropsFor(Verb verb)
        {
            var label = verb.verbProps.label;
            return string.IsNullOrEmpty(label) ? null : verbProps?.FirstOrDefault(prop => prop.label == label);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            foreach (var verbProp in verbProps) verbProp.ResolveReferences();
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

        public static DrawPosition Zero => new()
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