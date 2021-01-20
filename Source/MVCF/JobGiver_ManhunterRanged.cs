using RimWorld;
using Verse;
using Verse.AI;

// Mostly copied from ilikegoodfood's Verb Expansion Framework

namespace MVCF
{
    public class JobGiver_ManhunterRanged : ThinkNode_JobGiver
    {
        private const float TargetKeepRadius = 65f;
        private static readonly IntRange ExpiryIntervalShooterSucceeded = new IntRange(450, 550);

        protected override Job TryGiveJob(Pawn pawn)
        {
            var enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget != null && (enemyTarget.Destroyed ||
                                        Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 ||
                                        !pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly) ||
                                        (pawn.Position - enemyTarget.Position).LengthHorizontalSquared >
                                        TargetKeepRadius * TargetKeepRadius ||
                                        ((IAttackTarget) enemyTarget).ThreatDisabled(pawn))) enemyTarget = null;
            if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
            {
                enemyTarget = null;
            }
            else
            {
                enemyTarget = (Thing) AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat,
                                  x => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f,
                                  default, float.MaxValue, true) ??
                              (Thing) AttackTargetFinder.BestAttackTarget(pawn,
                                  TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                                  TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, t => t is Building, 0f,
                                  70f);

                if (enemyTarget is Pawn && enemyTarget.Faction == Faction.OfPlayer &&
                    pawn.Position.InHorDistOf(enemyTarget.Position, 40f))
                    Find.TickManager.slower.SignalForceNormalSpeed();
            }

            pawn.mindState.enemyTarget = enemyTarget;

            if (enemyTarget == null) return null;

            var verb = pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist);
            if (verb == null) return null;
            if (verb.IsMeleeAttack) return null;
            var positionIsStandable = pawn.Position.Standable(pawn.Map);
            var canHitEnemy = verb.CanHitTarget(enemyTarget);
            var enemyNear = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
            if ((positionIsStandable || enemyNear) && canHitEnemy)
                return new Job(JobDefOf.Wait_Combat, ExpiryIntervalShooterSucceeded.RandomInRange, true)
                {
                    canUseRangedWeapon = true
                };
            if (!CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = false,
                target = enemyTarget
            }, out var position))
                return null;

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
    }
}