using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEPirates;
using System.Security.Cryptography;

namespace VFECore
{

    [HarmonyPatch(typeof(HealthUtility), "DamageUntilDowned")]
    class Test_Patch
    {
        public static bool Prefix(Pawn p, bool allowBleedingWounds, DamageDef damage , ThingDef sourceDef, BodyPartGroupDef bodyGroupDef)
        {
            if (p.Downed)
            {
                return false;
            }
            HediffSet hediffSet = p.health.hediffSet;
            p.health.forceDowned = true;
            IEnumerable<BodyPartRecord> source = from x in HittablePartsViolence(hediffSet)
                                                 where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
                                                 select x;
            int num = 0;
            while (num < 300 && !p.Downed && source.Any())
            {
                num++;
                BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
                int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord));
                float statValue = p.GetStatValue(StatDefOf.IncomingDamageFactor);
                if (statValue > 0f)
                {
                    num2 = (int)((float)num2 / statValue);
                }
                num2 -= 3;
                if (num2 > 0 && (num2 >= 8 || num >= 250))
                {
                    if (num > 275)
                    {
                        num2 = Rand.Range(1, 8);
                    }
                    DamageDef damageDef = (damage != null) ? damage : ((bodyPartRecord.depth != BodyPartDepth.Outside) ? DamageDefOf.Blunt : ((allowBleedingWounds || !(bodyPartRecord.def.bleedRate > 0f)) ? HealthUtility.RandomViolenceDamageType() : DamageDefOf.Blunt));
                    int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
                    HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
                    if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3 * p.GetStatValue(StatDefOf.IncomingDamageFactor)))
                    {
                        DamageInfo dinfo = new DamageInfo(damageDef, num3, 999f, -1f, null, bodyPartRecord, null, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty: true, spawnFilth: true, QualityCategory.Normal, checkForJobOverride: false);
                        dinfo.SetAllowDamagePropagation(val: false);

                        /* Log.Message(p);
                         Log.Message(TakeDamageForked(p,dinfo));
                         Log.Message(TakeDamageForked(p,dinfo).hediffs.ToString());*/

                        Log.Message(p.apparel.WornApparel.ToStringSafeEnumerable());

                        foreach (Hediff hediff in TakeDamageForked(p, dinfo).hediffs)
                        {
                           
                            if (sourceDef != null)
                            {
                                hediff.sourceDef = sourceDef;
                            }
                            if (bodyGroupDef != null)
                            {
                                hediff.sourceBodyPartGroup = bodyGroupDef;
                            }
                        }
                    }
                }
            }
          if (p.Dead && !p.kindDef.forceDeathOnDowned)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
                for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
                {
                    stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i]);
                }
                Log.Error(stringBuilder.ToString());
            }
            p.health.forceDowned = false;

            

            return false;
        }


        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts()
                   where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
                   select x;
        }


        public static DamageWorker.DamageResult TakeDamageForked(Pawn p,DamageInfo dinfo)
        {
            if (p.Destroyed)
            {
                Log.Message("Returning because p.Destroyed");
                return new DamageWorker.DamageResult();
            }
            if (dinfo.Amount == 0f)
            {
                Log.Message("Returning because dinfo.Amount == 0f");
                return new DamageWorker.DamageResult();
            }
            if (p.def.damageMultipliers != null)
            {
                for (int i = 0; i < p.def.damageMultipliers.Count; i++)
                {
                    if (p.def.damageMultipliers[i].damageDef == dinfo.Def)
                    {
                        int num = Mathf.RoundToInt(dinfo.Amount * p.def.damageMultipliers[i].multiplier);
                        dinfo.SetAmount(num);
                    }
                }
            }
            p.PreApplyDamage(ref dinfo, out bool absorbed);
            if (absorbed)
            {
                Log.Message("Returning because absorbed");
                return new DamageWorker.DamageResult();
            }
            bool spawnedOrAnyParentSpawned = p.SpawnedOrAnyParentSpawned;
            Map mapHeld = p.MapHeld;
            DamageWorker.DamageResult damageResult = dinfo.Def.Worker.Apply(dinfo, p);
            if (dinfo.Def.harmsHealth && spawnedOrAnyParentSpawned)
            {
                mapHeld.damageWatcher.Notify_DamageTaken(p, damageResult.totalDamageDealt);
            }
            if (dinfo.Def.ExternalViolenceFor(p))
            {
                if (dinfo.SpawnFilth)
                {
                    GenLeaving.DropFilthDueToDamage(p, damageResult.totalDamageDealt);
                }
                if (dinfo.Instigator != null)
                {
                    Pawn pawn;
                    if ((pawn = (dinfo.Instigator as Pawn)) != null)
                    {
                        pawn.records.AddTo(RecordDefOf.DamageDealt, damageResult.totalDamageDealt);
                    }
                    if (dinfo.Instigator.Faction == Faction.OfPlayer)
                    {
                        QuestUtility.SendQuestTargetSignals(p.questTags, "TookDamageFromPlayer", p.Named("SUBJECT"), dinfo.Instigator.Named("INSTIGATOR"));
                    }
                }
                QuestUtility.SendQuestTargetSignals(p.questTags, "TookDamage", p.Named("SUBJECT"), dinfo.Instigator.Named("INSTIGATOR"), mapHeld.Named("MAP"));
            }
            p.PostApplyDamage(dinfo, damageResult.totalDamageDealt);
            Log.Message("Returning normally");
            return damageResult;
        }

    }


}