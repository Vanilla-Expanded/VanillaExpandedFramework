using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class QuestChainDef : Def
    {
        public string iconPath;
        public Texture2D icon;

        public override void PostLoad()
        {
            base.PostLoad();
            if (!iconPath.NullOrEmpty())
            {
                icon = ContentFinder<Texture2D>.Get(iconPath);
            }
        }
    }

    public class QuestChainExtension : DefModExtension
    {
        public QuestChainDef questChainDef;
        public List<QuestScriptDef> conditionSucceedQuests;
        public List<QuestScriptDef> conditionFailQuests;
        public IntRange ticksSinceSucceed;
        public IntRange ticksSinceFail;

        public QuestScriptDef conditionEither;
        public float conditionMinDaysSinceStart;
        public bool isRepeatable;
        public float mtbRepeat;
        public bool grantAgainOnFailure;
        public bool grantAgainOnExpiry;
        public float daysUntilGrantAgainOnFailure;
        public float daysUntilGrantAgainOnExpiry;
    }
}
