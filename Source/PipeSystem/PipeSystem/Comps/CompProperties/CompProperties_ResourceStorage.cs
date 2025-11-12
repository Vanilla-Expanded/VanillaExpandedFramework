using System;
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
        [LoadAlias("showOffMatWhenTransferring")]
        public bool showOffMatWhenTransfering = true;

        public float margin = 0.15f;
        public Vector2 barSize = new Vector2(1.3f, 0.4f);
        public Vector3 centerOffset = new Vector3(0, 0);
        public bool barHorizontal = false;
        public bool rotateBarWithBuilding = true;

        public ExtractOptions extractOptions;
        public RefillOptions refillOptions;
        [Obsolete($"Will be removed in the future, use {nameof(destroyOptions)} instead.")]
        public DestroyOption destroyOption;
        public List<DestroyOption> destroyOptions;

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

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);

            // Treat negative extract amount as "extract as much as possible"
            if (extractOptions is { extractAmount: < 0 })
                extractOptions.extractAmount = Mathf.CeilToInt(storageCapacity / extractOptions.ratio);

            // If a mod is using the (now outdated) destoy option, create the destroy options list (if not present) and add it there
#pragma warning disable CS0618 // Type or member is obsolete
            if (destroyOption != null)
            {
                destroyOptions ??= [];
                destroyOptions.Add(destroyOption);
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    public class ExtractOptions
    {
        internal Texture2D tex;

        /// <summary>
        /// The path to the gizmo's texture.
        /// </summary>
        public string texPath;
        /// <summary>
        /// The translation key for the gizmo's label.
        /// </summary>
        public string labelKey;
        /// <summary>
        /// The translation key to the gizmo's description.
        /// </summary>
        public string descKey;
        /// <summary>
        /// The translation key used if the gizmo is disabled due to not enough resource for extraction.
        /// </summary>
        public string disabledReasonKey;
        // Additional note: all the keys include 8 arguments you can use in translations:
        // {0} or {RESOURCEMIN} - the minimum amount of resource that's needed to extract it.
        // {1} or {RESOURCECOUNT} - the current amount of resource in storage.
        // {2} or {RESOURCEEXTRACTCOUNT} - the amount of resource that would be extracted to match the current {THINGCOUNT} count.
        // {3} or {RESOURCEMAX} - the maximum amount of resource we can extract at a time.
        // {4} or {THINGMIN} - the minimum amount of items we can extract.
        // {5} or {THINGCOUNT} - the current amount of items in storage.
        // {6} or {THINGEXTRACTCOUNT} - the current amount of items we can extract (same as above, included for consistency).
        // {7} or {THINGMAX} - the maximum amount of items we can extract at a time.
        // If using ratio of 1 all of those (besides {RESOURCEEXTRACTCOUNT}) will match 1-to-1.
        // If having extractExactAmount set to true, {RESOURCEMIN}/{THINGMIN} will match {RESOURCEMAX}/{THINGMAX}.

        /// <summary>
        /// How long it takes for a pawn (in ticks) to extract the fuel from the storage.
        /// </summary>
        public float extractTime = 150;
        /// <summary>
        /// The amount of items that will be extracted at a time.
        /// Using a negative value will automatically set it to the storage capacity.
        /// </summary>
        public int extractAmount;
        /// <summary>
        /// If true, the amount extracted will always exactly match extractAmount.
        /// If false, you'll be allowed to extract any amount (at least 1), but no more than extractAmount.
        /// </summary>
        public bool extractExactAmount = true;
        /// <summary>
        /// If both this is true the extractTime will be multiplied by the amount of extracted items.
        /// </summary>
        public bool extractTimeScalesWithAmount = false;
        /// <summary>
        /// The item that will be extracted from the storage.
        /// </summary>
        public ThingDef thing;

        /// <summary>
        /// The ratio of items extracted to resources in storage. Ratio of 2 means that extracting 1 item will drain 2 resources.
        /// </summary>
        public float ratio = 1;
    }

    public class RefillOptions
    {
        /// <summary>
        /// If false, a gizmo to enable/disable automatic refill will be included.
        /// If true, the gizmo won't be present and pawns will always refill the storage.
        /// </summary>
        public bool alwaysRefill = false;

        /// <summary>
        /// The amount of time (in ticks) it takes to fill this storage with an item.
        /// </summary>
        public float refillTime = 150;
        /// <summary>
        /// If this is true, the refillTime will be multiplied by the amount of inserted items.
        /// </summary>
        public bool refillTimeScalesWithAmount = false;
        /// <summary>
        /// The item that will be used to refill this storage.
        /// </summary>
        public ThingDef thing;

        /// <summary>
        /// The ratio of items inserted to resources put in the storage. Ratio of 2 means that inserting 1 item will put 2 resources in the storage.
        /// </summary>
        public float ratio = 1;
    }

    public class DestroyOption
    {
        /// <summary>
        /// An item, filth, or building that will be spawned when this storage is destroyed.
        /// For backwards compatibility reason, you can still call it "filth" in XML, but it's recommended to use "thing" instead.
        /// </summary>
        [LoadAlias("filth")]
        public ThingDef thing;
        /// <summary>
        /// The conversion ratio of fuel to spawned thing.
        /// For example, a ratio of 2 means that 1 resource will spawn 2 things, and a ratio of 0.5 means 2 resources will spawn 1 thing.
        /// This is opposite to refill options, so using a ratio of 2 for both will mean that refilling 1 thing will provide 2 resource, which will refund 1 thing again.
        /// Using a ratio of 0 will disable scaling the amount with the resource in storage.
        /// </summary>
        public float ratio = 1;
        /// <summary>
        /// The amount that will always be dropped, independently of the amount of resources in storage or the ratio.
        /// </summary>
        public int amount = 0;
        /// <summary>
        /// The maximum radius at which the things will be spawned at. For items and buildings, a value of 0 will ignore the radius. Filth requires a maximum radius.
        /// </summary>
        public int maxRadius;
        /// <summary>
        /// If the thing should be spawned when the storage is deconstructed by a pawn.
        /// </summary>
        public bool spawnWhenDeconstructed = true;
        /// <summary>
        /// If the thing should be spawned when the storage is destroyed.
        /// </summary>
        public bool spawnWhenDestroyed = true;
        /// <summary>
        /// If the thing should be spawned when the storage building is refunded (for example, if pit gate starts opening under it).
        /// It's a separate option from others in case we only give partial refund when deconstructed/destroyed, while full one for actual refund.
        /// </summary>
        public bool spawnWhenRefunded = true;
        /// <summary>
        /// When thing to spawn is a building, this determines if it should be spawned as a minified thing (if possible) or a building.
        /// </summary>
        public bool spawnMinified = true;
    }
}