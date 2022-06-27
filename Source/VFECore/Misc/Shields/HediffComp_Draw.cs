using UnityEngine;
using Verse;

// ReSharper disable once CheckNamespace
namespace VFECore.Shields
{
    public class HediffComp_Draw : HediffComp
    {
        public virtual void DrawAt(Vector3 drawPos)
        {
            Graphic?.Draw(drawPos, Pawn.Rotation, Pawn);
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (ShieldsSystem.HediffDrawsByPawn.TryGetValue(Pawn, out var list)) list.Add(this);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (ShieldsSystem.HediffDrawsByPawn.TryGetValue(Pawn, out var list)) list.Remove(this);
        }

        public virtual Graphic Graphic => (props as HediffCompProperties_Draw)?.graphic?.Graphic;
    }

    public class HediffCompProperties_Draw : HediffCompProperties
    {
        public GraphicData graphic;

        public override void PostLoad()
        {
            base.PostLoad();
            ShieldsSystem.ApplyDrawPatches();
        }
    }
}
