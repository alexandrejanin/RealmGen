using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WorldGen {
    public class WorldDisplay : MonoBehaviour {
        [SerializeField, OnValueChanged(nameof(OnDrawModeChanged))]
        private DrawMode drawMode;

        [TabGroup("Slope"), SerializeField] private bool drawSlope;
        [TabGroup("Slope"), SerializeField] private float slopeMultiplier;

        // Mesh
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private AnimationCurve heightCurve;

        [SerializeField, OnValueChanged(nameof(OnWorldGenerated))]
        private float heightMultiplier;

        // Gradients
        [SerializeField] private Gradient heightGradient, tempGradient, rainGradient;

        [SerializeField] private Material worldMaterial;

        private static readonly int WorldTextureId = Shader.PropertyToID("_WorldTexture");

        private Dictionary<DrawMode, Texture2D> textures;

        public void OnWorldGenerated() {
            var world = World.Current;

            meshFilter.mesh = MeshGenerator.GenerateTerrainMesh(world.heightMap, heightCurve,
                world.worldParameters.seaLevel,
                heightMultiplier);

            textures = new Dictionary<DrawMode, Texture2D>();
            textures.Add(DrawMode.Normal, GenerateClimateTexture());
            textures.Add(DrawMode.Height, GenerateTextureFromGradient(heightGradient, world.heightMap));
            textures.Add(DrawMode.Temperature, GenerateTextureFromGradient(tempGradient, world.tempMap));
            textures.Add(DrawMode.Rain, GenerateTextureFromGradient(rainGradient, world.rainMap));

            OnDrawModeChanged();
        }

        private void OnDrawModeChanged() {
            worldMaterial.SetTexture(WorldTextureId, textures[drawMode]);
        }

        private static Texture2D GenerateTextureFromGradient(Gradient gradient, float[,] map) {
            var worldSize = World.Current.worldParameters.worldSize;

            var pixels = new Color[worldSize * worldSize];
            for (var i = 0; i < worldSize * worldSize; i++) {
                pixels[i] = gradient.Evaluate(map[i % worldSize, i / worldSize]);
            }

            var texture = new Texture2D(worldSize, worldSize);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D GenerateClimateTexture() {
            var worldSize = World.Current.worldParameters.worldSize;

            var pixels = new Color[worldSize * worldSize];
            for (var i = 0; i < worldSize * worldSize; i++) {
                var x = i % worldSize;
                var y = i / worldSize;

                var climate = World.Current.climateMap[x, y];
                pixels[i] = climate.gradient.Evaluate(World.Current.heightMap[x, y]);
            }

            var texture = new Texture2D(worldSize, worldSize);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void OnDrawGizmos() {
            if (World.Current == null) return;

            var worldSize = World.Current.worldParameters.worldSize;

            if (drawSlope) {
                for (var y = 0; y < worldSize - 1; y++) {
                    for (var x = 0; x < worldSize - 1; x++) {
                        var slope = World.Current.slopeMap[x, y];

                        var startY = heightMultiplier * heightCurve.Evaluate(
                                         Mathf.InverseLerp(
                                             World.Current.worldParameters.seaLevel,
                                             1f,
                                             World.Current.heightMap[x, y]
                                         )
                                     );
                        var startPos = new Vector3(-(worldSize - 1) / 2f + x, startY, (worldSize - 1) / 2f - y);
                        var offset = new Vector3(slopeMultiplier * slope.x, 0, -slopeMultiplier * slope.y);

                        Gizmos.DrawLine(startPos, startPos + offset);
                    }
                }
            }
        }

        private enum DrawMode {
            Normal,
            Height,
            Temperature,
            Rain
        }
    }
}