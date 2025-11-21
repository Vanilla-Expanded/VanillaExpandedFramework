using Verse;
using RimWorld;


namespace VEF.Abilities
{
    class CompGiveHediff : CompAbilityEffect
    {

        new public CompProperties_GiveHediff Props
        {
            get
            {
                return (CompProperties_GiveHediff)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Props.applyToCaster)
            {
                parent.pawn.health.AddHediff(Props.hediffDef);
            }
            if (Props.applyToRadius)
            {
                foreach (Pawn pawn in parent.pawn.Map.mapPawns.AllPawnsSpawned)
                {

                    if (pawn.Spawned && pawn.Position.InHorDistOf(target.Cell, parent.def.EffectRadius))
                    {
                        pawn.health.AddHediff(Props.hediffDef);
                    }



                }


            }

        }






    }
}
