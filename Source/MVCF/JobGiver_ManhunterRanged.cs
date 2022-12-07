using MVCF.Features;
using RimWorld;
using Verse;
using Verse.AI;

// Mostly copied from ilikegoodfood's Verb Expansion Framework

namespace MVCF;

public class JobGiver_ManhunterRanged : ThinkNode_JobGiver
{
    private const float TargetKeepRadius = 65f;
    private static readonly IntRange ExpiryIntervalShooterSucceeded = new(450, 550);

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!MVCF.GetFeature<Feature_RangedAnimals>().Enabled) return null;
        var enemyTarget = pawn.mindState.enemyTarget;
        if (enemyTarget != null && (enemyTarget.Destroyed ||
                                    Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 ||
                                    !pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly) ||
                                    (pawn.Position - enemyTarget.Position).LengthHorizontalSquared >
                                    TargetKeepRadius * TargetKeepRadius ||
                                    ((IAttackTarget)enemyTarget).ThreatDisabled(pawn))) enemyTarget = null;
        if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
            enemyTarget = null;
        else if (enemyTarget == null)
            enemyTarget = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat,
                              x => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f,
                              default, float.MaxValue, true) ??
                          (Thing)AttackTargetFinder.BestAttackTarget(pawn,
                              TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                              TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, t => t is Building, 0f,
                              70f);

        pawn.mindState.enemyTarget = enemyTarget;

        if (enemyTarget == null) return null;

        var verb = pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist);
        if (verb == null) return null;
        if (verb.IsMeleeAttack) return null;

        if (verb.GetDamageDef() == DamageDefOf.Stun && enemyTarget is Pawn p && p.stances.stunner.Stunned)
            return null;

        if ((pawn.Position.Standable(pawn.Map) ||
             (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25) && verb.CanHitTarget(enemyTarget))
        {
            TryCauseTimeSlowdown(pawn, enemyTarget);
            return new Job(JobDefOf.Wait_Combat, ExpiryIntervalShooterSucceeded.RandomInRange, true)
            {
                canUseRangedWeapon = true
            };
        }

        if (!CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = false,
                target = enemyTarget
            }, out var position))
            return null;

        TryCauseTimeSlowdown(pawn, enemyTarget);

        if (position == pawn.Position)
            return new Job(JobDefOf.Wait_Combat, ExpiryIntervalShooterSucceeded.RandomInRange, true)
            {
                canUseRangedWeapon = true
            };

        return new Job(JobDefOf.Goto, position)
        {
            expiryInterval = ExpiryIntervalShooterSucceeded.RandomInRange,
            checkOverrideOnExpire = true
        };
    }

    private static void TryCauseTimeSlowdown(Pawn pawn, Thing enemyTarget)
    {
        if (enemyTarget is Pawn && enemyTarget.Faction == Faction.OfPlayer &&
            pawn.Position.InHorDistOf(enemyTarget.Position, 40f))
            Find.TickManager.slower.SignalForceNormalSpeed();
    }
}
