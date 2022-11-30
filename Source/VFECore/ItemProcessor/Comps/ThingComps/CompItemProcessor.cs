using Verse;

namespace ItemProcessor
{
    public class CompItemProcessor : ThingComp
    {
        //This class registers and de-registers buildings when they are built or destroyed
        //It calls methods in a map extender class, that is always instantiated per map 
        public CompProperties_ItemProcessor Props
        {
            get
            {
                return (CompProperties_ItemProcessor)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            ItemProcessor_MapComponent mapComp = this.parent.Map.GetComponent<ItemProcessor_MapComponent>();
            if (mapComp != null)
            {
                mapComp.AddItemProcessorToMap(this.parent);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            ItemProcessor_MapComponent mapComp = map.GetComponent<ItemProcessor_MapComponent>();
            if (mapComp != null)
            {
                mapComp.RemoveItemProcessorFromMap(this.parent);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {

            ItemProcessor_MapComponent mapComp = previousMap.GetComponent<ItemProcessor_MapComponent>();
            if (mapComp != null)
            {
                mapComp.RemoveItemProcessorFromMap(this.parent);
            }
        }
    }
}