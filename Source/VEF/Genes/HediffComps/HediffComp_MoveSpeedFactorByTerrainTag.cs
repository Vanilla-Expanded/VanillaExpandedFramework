using Verse;

namespace VEF.Genes;

public class HediffComp_MoveSpeedFactorByTerrainTag : HediffComp
{
    public HediffCompProperties_MoveSpeedFactorByTerrainTag Props => (HediffCompProperties_MoveSpeedFactorByTerrainTag)props;

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        AddThings();
    }

    public override void CompPostPostRemoved()
    {
        RemoveThings();
    }

    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        RemoveThings();
    }

    public override void Notify_PawnKilled()
    {
        RemoveThings();
    }

    public void AddThings()
    {
        if (parent.pawn != null && !Props.moveSpeedFactorByTerrainTag.NullOrEmpty())
            StaticCollectionsClass.AddMoveSpeedFactorByTerrainTag(parent.pawn, this, Props.moveSpeedFactorByTerrainTag);
    }

    public void RemoveThings()
    {
        if (parent.pawn != null && !Props.moveSpeedFactorByTerrainTag.NullOrEmpty())
            StaticCollectionsClass.RemoveMoveSpeedFactorByTerrainTag(parent.pawn, this);
    }
}