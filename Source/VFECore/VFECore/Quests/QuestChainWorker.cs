namespace VFECore
{
    public class QuestChainWorker
    {
        public QuestChainDef def;
        public virtual string GetDescription()
        {
            return def.description;
        }
    }
}