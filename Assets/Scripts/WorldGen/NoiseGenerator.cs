using UnityEngine;
using Random = System.Random;

namespace WorldGen {
    public static class NoiseGenerator {
        public static float[,] GenerateNoiseMap(
            int width,
            int height,
            int seed,
            NoiseParameters noiseParameters,
            bool normalize
        ) {
            var noiseMap = new float[width, height];
            var random = new Random(seed);
            var octaveOffsets = new Vector2[noiseParameters.octaves];
            float amplitude = 1;

            for (var i = 0; i < noiseParameters.octaves; i++) {
                float offsetX = random.Next(-99999, 99999);
                float offsetY = random.Next(-99999, 99999);
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
                amplitude *= noiseParameters.persistance;
            }

            var maxNoiseHeight = float.MinValue;
            var minNoiseHeight = float.MaxValue;

            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (var i = 0; i < noiseParameters.octaves; i++) {
                        var sampleX = (x - width / 2f + octaveOffsets[i].x) / noiseParameters.scale * frequency;
                        var sampleY = (y - height / 2f + octaveOffsets[i].y) / noiseParameters.scale * frequency;
                        var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;
                        amplitude *= noiseParameters.persistance;
                        frequency *= noiseParameters.lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight) {
                        maxNoiseHeight = noiseHeight;
                    }

                    if (noiseHeight < minNoiseHeight) {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            if (normalize) {
                for (var y = 0; y < height; y++) {
                    for (var x = 0; x < width; x++) {
                        noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                    }
                }
            }

            return noiseMap;
        }

        public static float Falloff(float value, float a, float b) =>
            Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));

        public static float[,] GetFalloffMap(int width, int height, float falloffA, float falloffB) {
            var map = new float[width, height];

            for (var i = 0; i < width; i++) {
                for (var j = 0; j < height; j++) {
                    var x = i / (float) width * 2 - 1;
                    var y = j / (float) height * 2 - 1;
                    var value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    var falloff = Falloff(value, falloffA, falloffB);
                    map[i, j] = falloff;
                }
            }

            return map;
        }
    }
}