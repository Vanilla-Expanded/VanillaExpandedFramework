
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;
using AnimalBehaviours;

namespace AnimalBehaviours
{
    class HediffComp_RemoveFromMechs : HediffComp
    {
        public HediffCompProperties_RemoveFromMechs Props
        {
            get
            {
                return (HediffCompProperties_RemoveFromMechs)this.props;
            }
        }



        public override void CompPostMake()
        {
            base.CompPostMake();
            if (this.parent.pawn.RaceProps.IsMechanoid)
            {
                this.parent.pawn.health.RemoveHediff(this.parent);
            }
        }




    }
}