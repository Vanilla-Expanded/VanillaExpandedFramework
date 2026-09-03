using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Weapons
{
    public class Projectile_Healing : Bullet
    {

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            base.Impact(hitThing, blockedByShield);
            Pawn pawn = hitThing as Pawn;
            if (pawn?.health != null)
            {

                List<Hediff_Injury> injuries = GetInjuries(pawn);
                if (injuries.Count > 0)
                {
                    Hediff_Injury injury = injuries.RandomElement();
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                    injury.Heal(0.5f);
                }
            }
        }

        public static List<Hediff_Injury> GetInjuries(Pawn pawn)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury hediff_Injury)
                {

                    injuries.Add(hediff_Injury);

                }
            }
            return injuries;
        }
    }
}
