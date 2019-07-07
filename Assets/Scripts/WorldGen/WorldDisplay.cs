using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WorldGen {
    public class WorldDisplay : MonoBehaviour {
        [SerializeField,
         OnValueChanged(nameof(OnDrawModeChanged))]
        private DrawMode drawMode;

        [TabGroup("Slope"),
         SerializeField]
        private bool drawSlope;

        [TabGroup("Slope"),
         SerializeField]
        private float slopeMultiplier;

        // Mesh
        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField,
         OnValueChanged(nameof(OnWorldGenerated))]
        private AnimationCurve heightCurve;

        [SerializeField,
         OnValueChanged(nameof(OnWorldGenerated))]
        private float heightMultiplier;

        // Gradients
        [SerializeField]
        private Gradient heightGradient, tempGradient, rainGradient;

        [SerializeField]
        private Material worldMaterial;

        private static readonly int WorldTextureId = Shader.PropertyToID("_WorldTexture");

        private Dictionary<DrawMode, Texture2D> textures;

        public void OnWorldGenerated() {
            var world = World.Current;
            
            meshFilter.mesh = MeshGenerator.GenerateTerrainMesh(
                world.HeightMap,
                heightCurve,
                world.WorldParameters.SeaLevel,
                heightMultiplier
            )[0, 0];

            textures = new Dictionary<DrawMode, Texture2D> {
                {DrawMode.Normal, GenerateClimateTexture()},
                {DrawMode.Height, GenerateTextureFromGradient(heightGradient, world.HeightMap)},
                {DrawMode.Temperature, GenerateTextureFromGradient(tempGradient, world.TempMap)},
                {DrawMode.Rain, GenerateTextureFromGradient(rainGradient, world.RainMap)}
            };

            OnDrawModeChanged();
        }

        private void OnDrawModeChanged() {
            worldMaterial.SetTexture(WorldTextureId, textures[drawMode]);
        }

        private static Texture2D GenerateTextureFromGradient(Gradient gradient, float[,] map) {
            var width = World.Current.WorldParameters.Width;
            var height = World.Current.WorldParameters.Height;

            var pixels = new Color[width * height];
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    pixels[x + y * width] = gradient.Evaluate(map[x, y]);
                }
            }

            var texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D GenerateClimateTexture() {
            var width = World.Current.WorldParameters.Width;
            var height = World.Current.WorldParameters.Height;

            var pixels = new Color[width * height];
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    Climate climate;

                    try {
                        climate = World.Current.GetClimate(x, y);
                    }
                    catch (System.IndexOutOfRangeException) {
                        Debug.Log(x + ", " + y);
                        throw;
                    }

                    pixels[x + y * width] = climate.gradient.Evaluate(World.Current.GetHeight(x, y));
                }
            }

            var texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void OnDrawGizmos() {
            if (!drawSlope || World.Current == null)
                return;

            var width = World.Current.WorldParameters.Width;
            var height = World.Current.WorldParameters.Height;

            for (var y = 0; y < height - 1; y++) {
                for (var x = 0; x < width - 1; x++) {
                    var slope = World.Current.GetSlope(x, y);

                    var startY = heightMultiplier * heightCurve.Evaluate(
                                     Mathf.InverseLerp(
                                         World.Current.WorldParameters.SeaLevel,
                                         1f,
                                         World.Current.GetHeight(x, y)
                                     )
                                 );
                    var startPos = new Vector3(-(width - 1) / 2f + x, startY, (height - 1) / 2f - y);
                    var offset = new Vector3(slopeMultiplier * slope.x, 0, -slopeMultiplier * slope.y);

                    Gizmos.DrawLine(startPos, startPos + offset);
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