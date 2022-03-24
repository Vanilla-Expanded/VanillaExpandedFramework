using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Create a new Graphic_LinkedPipe instance for each resource.
    /// Use the transmitterAtlas of the ResourceDef.
    /// Store them in a dictionnary for an easy access with the GetLinkedFor method.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class LinkedPipes
    {
        private static readonly Dictionary<PipeNetDef, Graphic_LinkedOverlayPipe> overlayLinked;

        private static readonly Dictionary<ThingDef, Graphic_LinkedPipe> pipesLinked;

        static LinkedPipes()
        {
            overlayLinked = new Dictionary<PipeNetDef, Graphic_LinkedOverlayPipe>();
            pipesLinked = new Dictionary<ThingDef, Graphic_LinkedPipe>();

            var netDefs = DefDatabase<PipeNetDef>.AllDefsListForReading;
            for (int i = 0; i < netDefs.Count; i++)
            {
                var netDef = netDefs[i];
                // Making overlay graphic
                Graphic graphicO = GraphicDatabase.Get<Graphic_Single>(netDef.overlayOptions.transmitterAtlas, ShaderDatabase.MetaOverlay);

                // If using the default overlay atlas, we need to color it
                if (netDef.overlayOptions.transmitterAtlas == "Special/PSTransmitterAtlas" && netDef.overlayOptions.overlayColor != null)
                    graphicO = graphicO.GetColoredVersion(graphicO.Shader, netDef.overlayOptions.overlayColor, netDef.overlayOptions.overlayColor);

                // Adding to the list
                overlayLinked.Add(netDef, new Graphic_LinkedOverlayPipe(graphicO, netDef));

                // Moving on to actual pipes linked graphics
                for (int o = 0; o < netDef.pipeDefs.Count; o++)
                {
                    var pipeDef = netDef.pipeDefs[o];

                    Graphic graphicP = GraphicDatabase.Get<Graphic_Single>(pipeDef.graphic.data.texPath, ShaderDatabase.Cutout);
                    pipesLinked.Add(pipeDef, new Graphic_LinkedPipe(graphicP, netDef));
                }
            }
        }

        public static Graphic_LinkedOverlayPipe GetOverlayFor(PipeNetDef resource) => overlayLinked[resource];

        public static Graphic_LinkedPipe GetPipeFor(ThingDef pipe) => pipesLinked[pipe];
    }
}