using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Noise;
using UnityEngine;

namespace VFECore.AI
{
    public class JobGiver_DraftedJobs : JobGiver_AIFightEnemy
    {
        public DraftedActionData actionData = null;
        public bool Hunt => actionData.hunt;
        protected override bool OnlyUseAbilityVerbs => !actionData.hunt;
        protected override bool OnlyUseRangedSearch => false;

        public List<AbilityDef> blacklist = new();

        // Not totally sure what this is used for, but it seems standard for these things. / Red.
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_DraftedJobs obj = (JobGiver_DraftedJobs)base.DeepCopy(resolve);
            //obj.skipIfCantTargetNow = skipIfCantTargetNow;
            return obj;
        }

        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            dest = pawn.Position;
            if (Hunt)
            {
                Thing enemyTarget = pawn.mindState.enemyTarget;
                if (CanTargetWithAbillities(pawn, enemyTarget, out Ability ability))
                {
                    CastPositionRequest newReq = default(CastPositionRequest);
                    newReq.caster = pawn;
                    newReq.target = enemyTarget;
                    newReq.verb = ability.verb;
                    newReq.maxRangeFromTarget = ability.verb.verbProps.range;
                    newReq.wantCoverFromTarget = false;
                    newReq.preferredCastPosition = pawn.Position;
                    return CastPositionFinder.TryFindCastPosition(newReq, out dest);
                }
            }
            return true;
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            if (pawn?.Drafted != true) return false;

            if (base.ExtraTargetValidator(pawn, target))
            {
                return Hunt || CanTargetWithAbillities(pawn, target, out _);
            }
            return false;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            // If IS is drated I think we can assume it is player controlled? I don't think non-player controlled pawns can be drafted? / Red.
            if (pawn?.Drafted != true)
            {
                return null;
            }

            actionData = DraftedActionHolder.GetData(pawn);
            if (!Hunt)
            { 
                if (actionData.autocastAbilities.Empty() || pawn.abilities?.abilities == null || pawn.abilities.abilities.NullOrEmpty())
                {
                    // If we're not in hunt mode, and there are literally no valid abilities, just let it continue to the regular indefinite wait job.
                    return null;
                }
                if (pawn.abilities.abilities.All(ability => !ability.CanCast ||
                    blacklist.Contains(ability.def)))
                {
                    return GetWaitForTimeJob(pawn, 100);
                }
            }

            var hostility = pawn.playerSettings.hostilityResponse;
            pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;   // Faster than messing around with all the checks against this.

            var draftedJob = GiveDraftedHuntJob(pawn); //base.TryGiveJob(pawn);

            pawn.playerSettings.hostilityResponse = hostility;
            if (draftedJob != null)
            {
                draftedJob.checkOverrideOnExpire = true;
                return draftedJob;
            }
            return GetWaitForTimeJob(pawn, 100);
        }

        protected static Job GetWaitForTimeJob(Pawn pawn, int ticks)
        {
            var waitForABit = JobMaker.MakeJob(JobDefOf.Wait_Combat, pawn.Position);
            waitForABit.expiryInterval = ticks;  // Set it so the job will start from the top after expiring
            waitForABit.checkOverrideOnExpire = true;
            return waitForABit;
        }

        protected override bool ShouldLoseTarget(Pawn pawn)
        {
            if (base.ShouldLoseTarget(pawn))
            {
                return true;
            }

            return !Hunt && !CanTargetWithAbillities(pawn, pawn.mindState.enemyTarget, out _);
        }


        protected bool CanTargetWithAbillities(Pawn pawn, Thing target, out Ability pickedAbility)
        {
            pickedAbility = null;
            if (pawn.Drafted == false || pawn.abilities?.abilities == null)
            {
                return false;
            }
            foreach (var ability in pawn.abilities.abilities)
            {
                pickedAbility = CanTargetWithAbility(target, ability);
                if (pickedAbility != null)
                {
                    return true;
                }
            }
            return false;
        }

        private Ability CanTargetWithAbility(Thing target, Ability ability)
        {
            if (!ability.CanCast)
            {
                return null;
            }
            if (blacklist.Contains(ability.def))
            {
                return null;
            }
            if (!ability.def.verbProperties.targetParams.CanTarget(target))
            {
                return null;
            }
            if (!ability.CanApplyOn((LocalTargetInfo)target))
            {
                return null;
            }

            if (ability.AICanTargetNow(target))
            {
                return ability;
            }

            return null;
        }

        protected Job GiveDraftedHuntJob(Pawn pawn)
        {
            UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget == null)
            {
                return null;
            }

            if (enemyTarget is Pawn enemyPawn && enemyPawn.IsPsychologicallyInvisible())
            {
                return null;
            }

            Job abilityJob = GetAbilityJob(pawn, enemyTarget);
            if (abilityJob != null)
            {
                return abilityJob;
            }

            // If not on hunt mode we want to stop here.
            if (!Hunt) return null;
            

            // We could toggle this on, but we would probably need to use a whitelist or something to prevent triple rocket launchers and such from being used.
            Verb verb = TryGetAttackVerb(pawn, enemyTarget, allowManualCastWeapons:false);
            if (verb == null)
            {
                return null;
            }

            if (verb.verbProps.IsMeleeAttack)
            {
                var meleeJob = MeleeAttackJob(pawn, enemyTarget);  // Make sure they still re-check for abilities, and etc.
                meleeJob.checkOverrideOnExpire = true;
                meleeJob.expiryInterval = 200;
                return meleeJob;
            }

            bool num = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
            bool standable = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
            bool cantHitTarget = verb.CanHitTarget(enemyTarget);
            bool closeEnough = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
            if ((num && standable && cantHitTarget) || (closeEnough && cantHitTarget))
            {
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
            }

            if (!TryFindShootingPosition(pawn, out var shootingPos))
            {
                return null;
            }

            if (shootingPos == pawn.Position)
            {
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
            }

            Job goToShootingPos = JobMaker.MakeJob(JobDefOf.Goto, shootingPos);
            goToShootingPos.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
            goToShootingPos.checkOverrideOnExpire = true;
            return goToShootingPos;
        }

        protected override void UpdateEnemyTarget(Pawn pawn)
        {
            Thing thing = pawn.mindState.enemyTarget;
            if (thing != null && ShouldLoseTarget(pawn))
            {
                thing = null;
            }

            if (thing == null)
            {
                thing = FindAttackTargetIfPossible(pawn);
                if (thing != null)
                {
                    Notify_EngagedTarget(pawn);
                    pawn.GetLord()?.Notify_PawnAcquiredTarget(pawn, thing);
                }
            }
            else
            {
                Thing thing2 = FindAttackTargetIfPossible(pawn);
                if (thing2 == null && !chaseTarget)
                {
                    thing = null;
                }
                else if (thing2 != null && thing2 != thing)
                {
                    Notify_EngagedTarget(pawn);
                    thing = thing2;
                }
            }

            pawn.mindState.enemyTarget = thing;
            Pawn enemy;
            if ((enemy = thing as Pawn) != null && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f) && !enemy.IsShambler && !pawn.IsPsychologicallyInvisible())
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
        }

        protected void Notify_EngagedTarget(Pawn pawn)  // This is marked internal for some reason...
        {
            pawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
        }

        protected new Thing FindAttackTargetIfPossible(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null, true) == null)
            {
                return null;
            }

            return FindAttackTarget(pawn);
        }

        // Use our own method for more aggressive behaviour.
        public Verb TryGetAttackVerb(Pawn pawn, Thing target, bool allowManualCastWeapons = false, bool allowOnlyManualCastWeapons = false)
        {
            if (allowManualCastWeapons) // Unlike the vanilla method, we make this PREFER manual cast stuff if enabled. Let's blow those cooldowns and charges!
            {
                if (pawn.equipment?.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb.Available() && pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast)
                {
                    return pawn.equipment.PrimaryEq.PrimaryVerb;
                }
                if (allowManualCastWeapons && pawn.apparel != null && pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast)
                {
                    Verb firstApparelVerb = pawn.apparel.FirstApparelVerb;
                    if (firstApparelVerb != null && firstApparelVerb.Available())
                    {
                        return firstApparelVerb;
                    }
                }
            }
            if (allowOnlyManualCastWeapons) return null;

            if (pawn.equipment?.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb.Available() && (!pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast))
            {
                return pawn.equipment.PrimaryEq.PrimaryVerb;
            }

            if (pawn.kindDef.canMeleeAttack)
            {
                return pawn.meleeVerbs.TryGetMeleeVerb(target);
            }

            return null;
        }

        private Job GetAbilityJob(Pawn pawn, Thing enemyTarget)
        {
            if (pawn.abilities == null)
            {
                return null;
            }

            List<Ability> list = pawn.abilities.AICastableAbilities(enemyTarget, offensive: true);
            if (list.NullOrEmpty())
            {
                return null;
            }
            // Filter all abilites not on the whitelist.
            
            list = list.Where(ability => CanTargetWithAbility(enemyTarget, ability) != null && ( actionData.AutoCastFor(ability.def))).ToList();
            if (list.Empty())
            {
                return null;
            }

            if (pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].verb.CanHitTarget(enemyTarget))
                    {
                        return list[i].GetJob(enemyTarget, enemyTarget);
                    }
                }

                for (int j = 0; j < list.Count; j++)
                {
                    LocalTargetInfo localTargetInfo = list[j].AIGetAOETarget();
                    if (localTargetInfo.IsValid)
                    {
                        return list[j].GetJob(localTargetInfo, localTargetInfo);
                    }
                }

                for (int k = 0; k < list.Count; k++)
                {
                    if (list[k].verb.targetParams.canTargetSelf)
                    {
                        return list[k].GetJob(pawn, pawn);
                    }
                }
            }

            return null;
        }
    }
}
