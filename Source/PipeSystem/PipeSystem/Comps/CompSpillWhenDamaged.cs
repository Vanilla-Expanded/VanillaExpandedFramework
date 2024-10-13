using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompSpillWhenDamaged : ThingComp
    {
        private CompResource compResource;
        private float hitPointToStart;
        private bool createFleck;
        private bool createFilth;

        private int atTick = 0;

        public CompProperties_SpillWhenDamaged Props => (CompProperties_SpillWhenDamaged)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compResource = parent.GetComp<CompResource>();
            hitPointToStart = parent.MaxHitPoints * Props.startAtHitPointsPercent;
            createFleck = Props.chooseFleckFrom.Count > 0;
            createFilth = Props.chooseFilthFrom.Count > 0;
            atTick = Find.TickManager.TicksGame + Props.spillEachTicks;
        }

        public override void CompTick()
        {
            int ticksGame = Find.TickManager.TicksGame;
            if (atTick < ticksGame)
            {
                atTick = ticksGame + Props.spillEachTicks;
                Spill();
            }
        }

        public override void CompTickRare()
        {
            int ticksGame = Find.TickManager.TicksGame;
            if (atTick < ticksGame)
            {
                atTick = ticksGame + Props.spillEachTicks;
                Spill();
            }
        }

        public override void CompTickLong()
        {
            int ticksGame = Find.TickManager.TicksGame;
            if (atTick < ticksGame)
            {
                atTick = ticksGame + Props.spillEachTicks;
                Spill();
            }
        }

        public void Spill()
        {
            if (parent.HitPoints < hitPointToStart)
            {
                var map = parent.Map;
                var pos = parent.Position;

                if (compResource.PipeNet is PipeNet p && p.Stored > Props.amountToDraw)
                {
                    p.DrawAmongStorage(Props.amountToDraw, p.storages);

                    if (createFilth && pos.Walkable(map))
                    {
                        ThingDef filth = Props.chooseFilthFrom.RandomElement();
                        if (RCellFinder.TryFindRandomCellNearWith(pos, i => i.Walkable(map) && FilthMaker.CanMakeFilth(i, map, filth), map, out IntVec3 cell, 1, Props.filthMaxSpawnRadius))
                            FilthMaker.TryMakeFilth(cell, map, filth, Props.filthAmountPerSpawn);
                    }
                }

                if (createFleck)
                {
                    FleckMaker.Static(pos, map, Props.chooseFleckFrom.RandomElement());
                }

                MoreSpillEffect(map, pos);
            }
        }

        public virtual void MoreSpillEffect(Map map, IntVec3 position)
        {
            return;
        }
    }
}
