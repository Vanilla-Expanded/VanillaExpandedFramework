using System;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class QuestChainDef : Def
    {
        public string iconPath;
        public Texture2D icon;
        public string questChainName;
        public Type workerClass;
        private QuestChainWorker cachedWorker;

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!iconPath.NullOrEmpty())
                {
                    icon = ContentFinder<Texture2D>.Get(iconPath);
                }
            });

            if (workerClass == null)
            {
                workerClass = typeof(QuestChainWorker);
            }

            cachedWorker = (QuestChainWorker)Activator.CreateInstance(workerClass);
            cachedWorker.def = this;
        }

        public QuestChainWorker Worker => cachedWorker;
    }

    public class QuestChainWorker
    {
        public QuestChainDef def;
        public virtual string GetDescription()
        {
            return def.description;
        }
    }
}