using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Outposts
{
    public class WorldComponent_OutpostAttacked: WorldComponent
    {
        //Adapated from Sarg's Raoming Monstosities    
        public int tickCounter;
        public int ticksToNextAssault = OutpostsMod.Settings.raidTimeInterval.RandomInRange;
        public WorldComponent_OutpostAttacked(World world) : base(world)
        {
        }
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (OutpostsMod.Settings.DoRaids)
            {
                if (Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Any())
                {
                    if (Current.Game.storyteller.difficultyDef != DifficultyDefOf.Peaceful)
                    {
                        if (tickCounter > ticksToNextAssault)
                        {                       
                            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, this.world);
                            parms.target = this.world;
                            IncidentDef def = Outposts_DefOf.VEF_OutpostAttacked;
                            def.Worker.TryExecute(parms);
                            ticksToNextAssault = OutpostsMod.Settings.raidTimeInterval.RandomInRange;
                            tickCounter = 0;                            
                        }
                        tickCounter++;
                    }
                    
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.tickCounter, nameof(this.tickCounter));
            Scribe_Values.Look(ref this.ticksToNextAssault, nameof(this.ticksToNextAssault));
        }
    }
}
