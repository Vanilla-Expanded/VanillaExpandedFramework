﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Contains all the informations of a new resource.
    /// We let the user choose unit, so it can be used with any resource tpe (gas, liquid, solid).
    /// </summary>
    public class PipeNetDef : Def
    {
        public Resource resource; // The resource used for this net.
        public List<ThingDef> pipeDefs; // The pipes used for this net.

        public DeconstructOption designator; // If this ins't null, we create a new designator that only deconstruct the given pipe.
        public OverlayOption overlayOptions; // Overlay option, with a default value

        public List<LinkOption> linkToRefuelables;

        public float transferAmount = 100f;
        public bool noStorageAlert = false;
        public bool foggedNetAlert = false;
        public List<ThingDef> alertProofDefs = new List<ThingDef>(); // If net is one building, and it's in this list, no alert

        internal Material offMat; // Off material

        public override IEnumerable<string> ConfigErrors()
        {
            List<string> errors = base.ConfigErrors().ToList();
            if (overlayOptions == null)
            {
                errors.Add($"overlayOptions can't be null. It's overlayColor tag is required.");
            }
            else
            {
                if (overlayOptions.overlayColor == null && overlayOptions.transmitterAtlas == "Special/PSTransmitterAtlas")
                    errors.Add($"overlayColor tag of overlayOptions can't be null.");
            }

            return errors;
        }
    }

    /// <summary>
    /// Contains informations about the resource used in the pipe net
    /// </summary>
    public class Resource
    {
        public string name; // Ressource name that should be used in report strings.
        public string unit; // Unit that should be used in report strings.
        public Color color; // Resource color, used in overlay/storage bar. New material created at startup.

        public bool onlyShowStored = false;
        public string offTexPath;
    }

    /// <summary>
    /// Contains the used information to create a designator that deconstruct given pipe.
    /// </summary>
    public class DeconstructOption
    {
        public string deconstructIconPath;
        public string shaderPath;
        public string maskTexturePath;
        public Color color;
        public Color colorTwo;
        public DesignationCategoryDef designationCategoryDef;
    }

    /// <summary>
    /// Some options for overlay
    /// </summary>
    public class OverlayOption
    {
        public string transmitterAtlas = "Special/PSTransmitterAtlas"; // The default atlas texture used for overlay

        // TODO: Add offset option
        // public Vector3 overlayOffset = new Vector3(0, 0, 0);
        public Color overlayColor;
    }

    public class LinkOption
    {
        public ThingDef thing;
        public int ratio = 1;
    }
}