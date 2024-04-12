using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public class CompGraphicByStyle : ThingComp
    {

        public int changeGraphicsCounter = 0;



        public CompProperties_GraphicByStyle Props
        {
            get
            {
                return (CompProperties_GraphicByStyle)this.props;
            }
        }

        public override void CompTick()
        {
            changeGraphicsCounter++;
            if (changeGraphicsCounter > Props.changeGraphicsInterval)
            {
                this.ChangeTheGraphics();
                changeGraphicsCounter = 0;
            }
            base.CompTick();
        }

        public void ChangeTheGraphics()
        {

            if (this.parent.Map != null && this.parent.Faction == Faction.OfPlayer && AnimalBehaviours_Settings.flagGraphicChanging)
            {
                Pawn pawn = this.parent as Pawn;
                pawn.Drawer.renderer.SetAllGraphicsDirty();

            }
        }
    }
}
