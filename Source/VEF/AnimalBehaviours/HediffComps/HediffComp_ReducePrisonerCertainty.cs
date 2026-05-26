
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_ReducePrisonerCertainty : HediffComp
    {
        public HediffCompProperties_ReducePrisonerCertainty Props
        {
            get
            {
                return (HediffCompProperties_ReducePrisonerCertainty)this.props;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Pawn.IsHashIntervalTick(Props.checkingInterval, delta))
            {
                if (Pawn.RaceProps.Humanlike && Pawn.Ideo !=null && Current.Game.World.factionManager.OfPlayer?.ideos?.PrimaryIdeo!=null)
                {
                    if (Pawn.Ideo != Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo)
                    {
                        Pawn.ideo.Reassure(-Props.certaintyPerTick * Props.checkingInterval /100);
                        if (Pawn.ideo.Certainty <= 0)
                        {
                            Pawn.ideo.SetIdeo(Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo);
                        }
                    }
                    else
                    {
                        Pawn.ideo.Reassure(Props.certaintyPerTick * Props.checkingInterval / 100);
                    }
                }            
            }
        }
    }
}
