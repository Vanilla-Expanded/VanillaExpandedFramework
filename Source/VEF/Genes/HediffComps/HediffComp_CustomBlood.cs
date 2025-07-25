﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace VEF.Genes
{

    public class HediffCompProperties_CustomBlood : HediffCompProperties
    {
        public ThingDef customBloodThingDef = null;
        public string customBloodIcon = "";
        public EffecterDef customBloodEffect = null;
        public FleshTypeDef customWoundsFromFleshtype = null;

        public HediffCompProperties_CustomBlood()
        {
            this.compClass = typeof(HediffComp_CustomBlood);
        }
    }

    public class HediffComp_CustomBlood : HediffComp
	{

		public HediffCompProperties_CustomBlood Props => (HediffCompProperties_CustomBlood)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            AddThings();
        }

        public override void CompPostPostRemoved()
        {
            RemoveThings();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            RemoveThings();
        }

        public override void Notify_PawnKilled()
        {
            RemoveThings();
        }

        public void AddThings()
        {
            if (parent.pawn != null)
            {
                
                if (Props.customBloodThingDef != null)
                {
                    StaticCollectionsClass.AddBloodtypeGenePawnToList(parent.pawn, Props.customBloodThingDef);
                }
                if (Props.customBloodIcon != "")
                {
                    StaticCollectionsClass.AddBloodIconGenePawnToList(parent.pawn, Props.customBloodIcon);
                }
                if (Props.customBloodEffect != null)
                {
                    StaticCollectionsClass.AddBloodEffectGenePawnToList(parent.pawn, Props.customBloodEffect);
                }
                if (Props.customWoundsFromFleshtype != null)
                {
                    StaticCollectionsClass.AddWoundsFromFleshtypeGenePawnToList(parent.pawn, Props.customWoundsFromFleshtype);
                }
            }
        }

        public void RemoveThings()
        {
            if (parent.pawn != null)
            {
               
                if (Props.customBloodThingDef != null)
                {
                    StaticCollectionsClass.RemoveBloodtypeGenePawnFromList(parent.pawn);
                }
                if (Props.customBloodIcon != "")
                {
                    StaticCollectionsClass.RemoveBloodIconGenePawnFromList(parent.pawn);
                }
                if (Props.customBloodEffect != null)
                {
                    StaticCollectionsClass.RemoveBloodEffectGenePawnFromList(parent.pawn);
                }
                if (Props.customWoundsFromFleshtype != null)
                {
                    StaticCollectionsClass.RemoveWoundsFromFleshtypeGenePawnFromList(parent.pawn);
                }
            }
        }

    }
}
