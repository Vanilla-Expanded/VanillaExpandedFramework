using Verse;

namespace VEF.Things;

public class DamageWorker_ExtinguishCustomFire : DamageWorker
{
    public const float DamageAmountToFireSizeRatio = 0.01f;

    protected CustomFireDamageExtension Extension => field ??= def.GetModExtension<CustomFireDamageExtension>();

    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        var damageResult = new DamageResult();
        var fire = victim as CustomFire;
        if (fire == null || fire.Destroyed)
        {
            var thing = victim?.GetAttachment(Extension.fireDef);
            if (thing != null)
                fire = (CustomFire)thing;
        }

        if (fire is { Destroyed: false })
        {
            base.Apply(dinfo, victim);
            fire.fireSize -= dinfo.Amount * DamageAmountToFireSizeRatio;
            if (fire.fireSize < 0.1f)
                fire.Destroy();
        }

        if (victim is Pawn pawn)
        {
            var hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn);
            hediff.Severity = dinfo.Amount;
            pawn.health.AddHediff(hediff, null, dinfo);
        }

        return damageResult;
    }
}