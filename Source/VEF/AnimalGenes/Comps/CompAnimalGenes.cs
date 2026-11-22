using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using VEF.AnimalGenes;
using Verse;
using Verse.AI;

namespace VEF.AnimalGenes
{
    public class CompAnimalGenes : ThingComp
    {
        public FeratypeDef feratype;
        public List<AnimalGeneDef> genes = new List<AnimalGeneDef>();
        public float cachedLifespanFactor = -1;
        private bool feratypeApplied;
        private bool inheritedGenesAssigned;
        public bool isAlpha;
        public int soloTicks;
        public int ticksSinceLastDisease = 0;
        public bool active = true;
        public bool autoslaughterActive = false;

        public CompProperties_AnimalGenes Props => (CompProperties_AnimalGenes)props;
        public bool HasAssignedGenes => inheritedGenesAssigned || genes.Count != 0;

        public float LifeSpanFactor
        {
            get
            {
                if (cachedLifespanFactor == -1)
                {
                    int totalStability = 0;
                    foreach (AnimalGeneDef gene in genes)
                    {
                        totalStability += gene.stability;
                    }

                    cachedLifespanFactor = (float)(1 - (0.5 / Math.Log(21) * Math.Log(Math.Abs(totalStability) + 1)));
                }
                return cachedLifespanFactor;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
   
            if (!respawningAfterLoad && !feratypeApplied)
                ApplyFeratype();

        }

        public void ApplyFeratype()
        {
            Pawn pawn = parent as Pawn;
            PawnKindDef kindDef = pawn?.kindDef ?? parent.TryGetComp<CompHatcher>()?.Props.hatcherPawn;
            feratype ??= FindFeratype(kindDef);
            feratypeApplied = true;

            // Eggs and hatchlings can receive their genes before they spawn.
            if (feratype == null || HasAssignedGenes)
                return;

            foreach (AnimalGeneDef gene in feratype.animalGenes)
                AnimalGeneUtility.AddGene(this, gene);
            AnimalGeneUtility.HandleMutations(this, pawn);
        }

        internal static FeratypeDef FindFeratype(PawnKindDef kindDef) =>
            kindDef == null ? null : DefDatabase<FeratypeDef>.AllDefsListForReading.FirstOrDefault(def => def.race == kindDef);

        public void SetInheritedGenes(IEnumerable<AnimalGeneDef> inheritedGenes)
        {
            // Copy the list so the egg and its hatchlings can change independently.
            genes = inheritedGenes.Distinct().ToList();
            inheritedGenesAssigned = true;
            ResetCaches();
        }

        public void ResetCaches()
        {
            cachedLifespanFactor = -1;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref feratype, "feratype");
            Scribe_Values.Look(ref feratypeApplied, "feratypeApplied");
            Scribe_Values.Look(ref inheritedGenesAssigned, "inheritedGenesAssigned");
            Scribe_Values.Look(ref ticksSinceLastDisease, "ticksSinceLastDisease",0);
            Scribe_Collections.Look(ref genes, "animalGenes", LookMode.Def);
            Scribe_Values.Look(ref isAlpha, "isAlpha", false);
            Scribe_Values.Look(ref soloTicks, "soloTicks", 0);
            Scribe_Values.Look(ref autoslaughterActive, "autoslaughterActive", false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                genes ??= new List<AnimalGeneDef>();
                if (!feratypeApplied || (!inheritedGenesAssigned && genes.Count == 0))
                    ApplyFeratype();
            }
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (isAlpha)
            {
                BecomeAlpha(false);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            if(parent is Pawn pawn && pawn.IsColonyAnimal)
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "VRE_PreventSlaughter".Translate();
                command_Toggle.defaultDesc = "VRE_PreventSlaughterDesc".Translate();
                command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Gizmo/PreventSlaughter", true);
              
                command_Toggle.isActive = () => autoslaughterActive;
                command_Toggle.toggleAction = delegate
                {
                    autoslaughterActive = !autoslaughterActive;

                };
                yield return command_Toggle;
            }
            
            if (DebugSettings.ShowDevGizmos)
            {

                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Do birth";
                command_Action.action = delegate
                {
                    Pawn pawn = parent as Pawn;
                    Hediff pregnantHediff = pawn?.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant);
                    if (pregnantHediff != null)
                    {
                        pregnantHediff.Severity = 1;
                    }
                    else Messages.Message("VRE_NotPregnant".Translate(), pawn, MessageTypeDefOf.RejectInput);

                };
                yield return command_Action;


            }
        }

        public void BecomeAlpha(bool alpha)
        {
            isAlpha = alpha;
            if (!alpha)
            {
                soloTicks = 0;
            }
            var pawn = (Pawn)parent;
            var suffix = " (" + "VRE_Alpha".Translate() + ")";
            var name = pawn.Name.ToString().Replace(suffix, "");
            if (alpha)
            {
                pawn.Name = new NameSingle(name + suffix, false);
            }
            else
            {
                bool numerical = name.Length > 0 && char.IsDigit(name[name.Length - 1]);
                pawn.Name = new NameSingle(name, numerical);
            }
        }

        public override void CompTick()
        {
            ticksSinceLastDisease++;
            base.CompTick();
        }

        
    }
}
