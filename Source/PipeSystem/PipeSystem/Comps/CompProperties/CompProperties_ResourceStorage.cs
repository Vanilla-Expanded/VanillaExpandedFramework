using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ResourceStorage : CompProperties_Resource
    {
        public CompProperties_ResourceStorage()
        {
            compClass = typeof(CompResourceStorage);
        }

        public float storageCapacity; // Maximum amount of resource to be stored

        public bool drawStorageBar = true; // Should draw the storage bar on screen?
        public bool addStorageInfo = true; // Should we add storage info on comp inspect
        public bool addTransferGizmo = true;
        public bool showOffMatWhenTransfering = true;

        public float margin = 0.15f;
        public Vector2 barSize = new Vector2(1.3f, 0.4f);
        public Vector3 centerOffset = new Vector3(0, 0);

        public ExtractOptions extractOptions;
        public RefillOptions refillOptions;
        public DestroyOption destroyOption;

        public bool contentRequirePower = false;
        public bool preventRotInNegativeTemp = true;
        public float daysToRotStart = 0.75f;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;

            if (contentRequirePower && parentDef.tickerType == TickerType.Never)
                yield return $"{parentDef.defName} CompProperties_ResourceStorage is using contentRequirePower and need a ticker type (any)";
        }
    }

    public class ExtractOptions
    {
        internal Texture2D tex;

        public string texPath;
        public string labelKey;
        public string descKey;

        public int extractTime = 150;
        public int extractAmount;
        public ThingDef thing;

        public int ratio = 1;
    }

    public class RefillOptions
    {
        public bool alwaysRefill = false;

        public int refillTime = 150;
        public ThingDef thing;

        public int ratio = 1;
    }

    public class DestroyOption
    {
        public ThingDef filth;
        public int ratio = 1;
        public int maxRadius;
    }
}