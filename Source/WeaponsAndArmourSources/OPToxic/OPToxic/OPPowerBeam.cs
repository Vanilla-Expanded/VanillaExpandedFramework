using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace OPToxic
{
    public class OPPowerBeam : OrbitalStrike
    {
        public override void StartStrike()
        {
            base.StartStrike();
        }

        public override void Tick()
        {
            base.Tick();
            if (!Destroyed)
            {
                ThingDef def = this.def;
                int num = OPBeamDefGetValue.OPBeamGetNumFires(def);
                if (num < 1)
                {
                    num = 1;
                }
                if (num > 5)
                {
                    num = 5;
                }
                for (int i = 0; i < num; i++)
                {
                    StartRandomFireAndDoFlameDamage(def);
                }
            }
        }

        private void StartRandomFireAndDoFlameDamage(ThingDef OPBeamDef)
        {
            float EffRadius = OPBeamDefGetValue.OPBeamGetRadius(OPBeamDef);
            if (EffRadius < 1f)
            {
                EffRadius = 1f;
            }
            if (EffRadius > 15f)
            {
                EffRadius = 15f;
            }
            IntVec3 c = (from x in GenRadial.RadialCellsAround(Position, EffRadius, true)
                         where x.InBounds(Map)
                         select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(Position) / EffRadius, 1f) + 0.05f);
            FireUtility.TryStartFireIn(c, Map, Rand.Range(0.1f, 0.5f),null);
            tmpThings.Clear();
            tmpThings.AddRange(c.GetThingList(Map));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                int num = (!(tmpThings[i] is Corpse)) ? FlameDamageAmountRange.RandomInRange : CorpseFlameDamageAmountRange.RandomInRange;
                float num2 = OPBeamDefGetValue.OPBeamGetDmgFact(OPBeamDef);
                if (num2 > 2f)
                {
                    num2 = 2f;
                }
                if (num2 < 0.1f)
                {
                    num2 = 0.1f;
                }
                num = (int)(num * num2);
                if (num < 1)
                {
                    num = 1;
                }
                if (num > 99)
                {
                    num = 99;
                }
                Pawn pawn = tmpThings[i] as Pawn;
                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                if (pawn != null)
                {
                    battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_PowerBeam, this.instigator as Pawn);
                    Find.BattleLog.Add(battleLogEntry_DamageTaken);
                }
                Thing thing = tmpThings[i];
                DamageDef flame = DamageDefOf.Flame;
                float num3 = num;
                Thing instigator = this.instigator;
                ThingDef weaponDef = this.weaponDef;
                thing.TakeDamage(new DamageInfo(flame, num3, 0f, -1f, instigator, null, weaponDef, DamageInfo.SourceCategory.ThingOrUnknown, null)).AssociateWithLog(battleLogEntry_DamageTaken);
            }
            tmpThings.Clear();
        }

        public const float Radius = 8f;

        private const int FiresStartedPerTick = 3;

        private static readonly IntRange FlameDamageAmountRange = new IntRange(25, 50);

        private static readonly IntRange CorpseFlameDamageAmountRange = new IntRange(3, 5);

        private static readonly List<Thing> tmpThings = new List<Thing>();
    }
}