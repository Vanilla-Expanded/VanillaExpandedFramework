using System;
using System.Collections.Generic;
using RimWorld;
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
        public bool isAlpha;
        public int soloTicks;

        public new CompProperties_AnimalGenes Props => (CompProperties_AnimalGenes)props;

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

            WorldComponent_AnimalGenes.Instance.AddAnimalComp(parent, this);

            if (!respawningAfterLoad && !feratypeApplied)
            {
                ApplyFeratype();
                feratypeApplied = true;
            }

        }

        public void ApplyFeratype()
        {
            Pawn pawn = parent as Pawn;
            PawnKindDef kindDef = pawn?.kindDef ?? parent.TryGetComp<CompHatcher>()?.Props.hatcherPawn;

            foreach (FeratypeDef feratypeIterator in DefDatabase<FeratypeDef>.AllDefsListForReading)
            {
                if (kindDef != null)
                {
                    if (feratypeIterator.race == kindDef)
                    {
                        feratype = feratypeIterator;
                        foreach (AnimalGeneDef gene in feratype.animalGenes)
                        {
                            AnimalGeneUtility.AddGene(this, gene);
                        }
                        AnimalGeneUtility.HandleMutations(this, pawn);
                    }
                }

            }
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
            Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
            Scribe_Values.Look(ref isAlpha, "isAlpha", false);
            Scribe_Values.Look(ref soloTicks, "soloTicks", 0);

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
    }
}
