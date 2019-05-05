using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Random = System.Random;

namespace WorldGen {
    public static class NoiseGenerator {
        public static float[,] GenerateNoiseMap(int size, int seed, NoiseParameters noiseParameters) {
            var noiseMap = new float[size, size];
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

            for (var y = 0; y < size; y++) {
                for (var x = 0; x < size; x++) {
                    amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (var i = 0; i < noiseParameters.octaves; i++) {
                        var sampleX = (x - size / 2f + octaveOffsets[i].x) / noiseParameters.scale * frequency;
                        var sampleY = (y - size / 2f + octaveOffsets[i].y) / noiseParameters.scale * frequency;
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

            for (var y = 0; y < size; y++) {
                for (var x = 0; x < size; x++) {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }

        public static float Falloff(float value, float a, float b) =>
            Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));

        public static float[,] GetFalloffMap(int size, float falloffA, float falloffB) {
            var map = new float[size, size];
            for (var i = 0;
                i < size;
                i++) {
                for (var j = 0; j < size; j++) {
                    var x = i / (float) size * 2 - 1;
                    var y = j / (float) size * 2 - 1;
                    var value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    var falloff = Falloff(value, falloffA, falloffB);
                    map[i, j] = falloff;
                }
            }

            return map;
        }

        public static float[,] Blur(float[,] map, int iterations = 1) {
            var xLength = map.GetLength(0);
            var yLength = map.GetLength(1);

            var newMap = new float[xLength, yLength];

            for (var y = 1; y < yLength - 1; y++) {
                for (var x = 1; x < xLength - 1; x++) {
                    newMap[x, y] = (map[x - 1, y - 1] + map[x, y - 1] + map[x + 1, y - 1] +
                                    map[x - 1, y] + map[x, y] + map[x + 1, y] +
                                    map[x - 1, y + 1] + map[x, y + 1] + map[x + 1, y + 1]) / 9f;
                }
            }

            return newMap;
        }
    }
}