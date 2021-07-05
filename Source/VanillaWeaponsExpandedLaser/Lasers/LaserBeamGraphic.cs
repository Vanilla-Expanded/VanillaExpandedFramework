using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VanillaWeaponsExpandedLaser
{
    class LaserBeamGraphic :Thing
    {
        new LaserBeamDef def => base.def as LaserBeamDef;

        int ticks;
        int colorIndex = 2;
        Vector3 a;
        Vector3 b;

        public Matrix4x4 drawingMatrix = default(Matrix4x4);
        Material materialBeam;
        Mesh mesh;


        public float Opacity => (float)Math.Sin(Math.Pow(1.0 - 1.0 * ticks / def.lifetime, def.impulse) * Math.PI);
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref colorIndex, "colorIndex");
            Scribe_Values.Look(ref a, "a");
            Scribe_Values.Look(ref b, "b");
        }

        public override void Tick()
        {
            if (def==null || ticks++ > def.lifetime)
            {
                Destroy(DestroyMode.Vanish);
            }
        }

        void SetColor(Thing launcher)
        {
            IBeamColorThing gun = null;

            Pawn pawn = launcher as Pawn;
            if (pawn != null && pawn.equipment != null) gun = pawn.equipment.Primary as IBeamColorThing;
            if (gun == null) gun = launcher as IBeamColorThing;

            if (gun != null && gun.BeamColor != -1)
            {
                colorIndex = gun.BeamColor;
            }
        }

        public void Setup(Thing launcher, Vector3 origin, Vector3 destination)
        {
            SetColor(launcher);

            a = origin;
            b = destination;
        }

        public void SetupDrawing()
        {
            if (mesh != null) return;

            materialBeam = def.GetBeamMaterial(colorIndex) ?? def.graphicData.Graphic.MatSingle;

            if (this.def.graphicData.graphicClass == typeof(Graphic_Random))
            {
                materialBeam = def.GetBeamMaterial(Rand.RangeInclusive(0, this.def.materials.Count)) ?? def.graphicData.Graphic.MatSingle;
            }
            float beamWidth = def.beamWidth;
            Quaternion rotation = Quaternion.LookRotation(b - a);
            Vector3 dir = (b - a).normalized;
            float length = (b - a).magnitude;

            Vector3 drawingScale = new Vector3(beamWidth, 1f, length);
            Vector3 drawingPosition = (a + b) / 2;
            drawingMatrix.SetTRS(drawingPosition, rotation, drawingScale);

            float textureRatio = 1.0f * materialBeam.mainTexture.width / materialBeam.mainTexture.height;
            float seamTexture = def.seam < 0 ? textureRatio : def.seam;
            float capLength = beamWidth / textureRatio / 2f * seamTexture;
            float seamGeometry = length <= capLength * 2 ? 0.5f : capLength * 2 / length;

            this.mesh = MeshMakerLaser.Mesh(seamTexture, seamGeometry);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (def==null || def.decorations == null || respawningAfterLoad) return;

            foreach (var decoration in def.decorations)
            {
                float spacing = decoration.spacing * def.beamWidth;
                float initalOffset = decoration.initialOffset * def.beamWidth;

                Vector3 dir = (b - a).normalized;
                float angle = (b - a).AngleFlat();
                Vector3 offset = dir * spacing;
                Vector3 position = a + offset * 0.5f + dir * initalOffset;
                float length = (b - a).magnitude - spacing;

                int i = 0;
                while (length > 0)
                {
                    MoteLaserDectoration moteThrown = ThingMaker.MakeThing(decoration.mote, null) as MoteLaserDectoration;
                    if (moteThrown == null) break;

                    moteThrown.beam = this;
                    moteThrown.airTimeLeft = def.lifetime;
                    moteThrown.Scale = def.beamWidth;
                    moteThrown.exactRotation = angle;
                    moteThrown.exactPosition = position;
                    moteThrown.SetVelocity(angle, decoration.speed);
                    moteThrown.baseSpeed = decoration.speed;
                    moteThrown.speedJitter = decoration.speedJitter;
                    moteThrown.speedJitterOffset = decoration.speedJitterOffset * i;
                    GenSpawn.Spawn(moteThrown, a.ToIntVec3(), map, WipeMode.Vanish);

                    position += offset;
                    length -= spacing;
                    i++;
                }
            }
        }

        public override void Draw()
        {
            SetupDrawing();

            float opacity = Opacity;
            if (this.def.graphicData.graphicClass == typeof(Graphic_Flicker))
            {
                if (!Find.TickManager.Paused && Find.TickManager.TicksGame % this.def.flickerFrameTime == 0)
                {
                    materialBeam = def.GetBeamMaterial(Rand.RangeInclusive(0, this.def.materials.Count)) ?? def.graphicData.Graphic.MatSingle;
                }
            }
            Graphics.DrawMesh(mesh, drawingMatrix, FadedMaterialPool.FadedVersionOf(materialBeam, opacity), 0);
        }
    }
}
