using HarmonyLib;
using RimWorld;
using System;
using Verse;
using UnityEngine;


namespace VEF.Graphics
{
    public class ReflectionCache
    {
        public static readonly AccessTools.FieldRef<Thing, Graphic> itemGraphic =
           AccessTools.FieldRefAccess<Thing, Graphic>(AccessTools.Field(typeof(Thing), "graphicInt"));

        public static readonly Func<Pawn, bool> canOrderPlayerPawn = (Func<Pawn, bool>)Delegate.CreateDelegate(typeof(Func<Pawn, bool>),
            AccessTools.Method(typeof(PawnAttackGizmoUtility), "CanOrderPlayerPawn"));

        public static readonly AccessTools.FieldRef<Graphic_Single, Material> graphicMat =
           AccessTools.FieldRefAccess<Graphic_Single, Material>(AccessTools.Field(typeof(Graphic_Single), "mat"));

        public static readonly AccessTools.FieldRef<CompGeneratedNames, string> compGeneratedNamesName =
           AccessTools.FieldRefAccess<CompGeneratedNames, string>(AccessTools.Field(typeof(CompGeneratedNames), "name"));
    }
}
