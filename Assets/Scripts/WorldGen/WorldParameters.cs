using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace WorldGen {
    [Serializable]
    public class WorldParameters : SerializedScriptableObject {
        [Range(16, 513)] public int worldSize;

        [TabGroup("Height"), SerializeField] private NoiseParameters heightParameters;

        [TabGroup("Height"), Range(0, 10), SerializeField]
        private float falloffA, falloffB;

        [TabGroup("Height"), Range(0, 1), SerializeField]
        private float falloffMultiplier;

        [TabGroup("Rain"), SerializeField] private NoiseParameters rainParameters;

        [TabGroup("Temperature"), Range(0, 10), SerializeField]
        private float tempA, tempB;

        [TabGroup("Temperature"), Range(0, 1), SerializeField]
        private float tempHeightRatio;

        [TabGroup("Climates"), Range(0, 1)] public float seaLevel, mountainLevel;
        [TabGroup("Climates"), SerializeField] private Climate seaClimate;
        [TabGroup("Climates"), SerializeField] private Climate mountainClimate;

        [TabGroup("Climates"), TableMatrix(HorizontalTitle = "Temperature", VerticalTitle = "Rain"), SerializeField]
        private Climate[,] climateTable;

        public float[,] GetHeightMap(int seed) {
            var heightMap = NoiseGenerator.GenerateNoiseMap(worldSize, seed, heightParameters);

            var falloffMap = NoiseGenerator.GetFalloffMap(worldSize, falloffA, falloffB);
            for (var y = 0; y < worldSize; y++) {
                for (var x = 0; x < worldSize; x++) {
                    heightMap[x, y] = Mathf.Clamp01(heightMap[x, y] - falloffMap[x, y] * falloffMultiplier);
                }
            }

            return heightMap;
        }

        public Vector2[,] GetSlopeMap(float[,] heightMap) {
            var slopeMap = new Vector2[worldSize, worldSize];

            for (var y = 1; y < worldSize - 1; y++) {
                for (var x = 1; x < worldSize - 1; x++) {
                    if (heightMap[x, y] < seaLevel) continue;

                    var xSlope = heightMap[x + 1, y] - heightMap[x - 1, y];
                    var ySlope = heightMap[x, y + 1] - heightMap[x, y - 1];

                    var thing = Mathf.Sqrt(xSlope * xSlope + ySlope * ySlope + 1);

                    var normal = new Vector2(-xSlope, -ySlope) / thing;

                    slopeMap[x, y] = normal;
                }
            }

            return slopeMap;
        }

        public float[,] GetRainMap(int seed) {
            return NoiseGenerator.GenerateNoiseMap(worldSize, seed, rainParameters);
        }

        public float[,] GetTempMap(float[,] heightMap) {
            var tempMap = new float[worldSize, worldSize];

            for (var y = 0; y < worldSize; y++) {
                for (var x = 0; x < worldSize; x++) {
                    var gradientTemp = y / (float) worldSize;
                    tempMap[x, y] = Mathf.Lerp(NoiseGenerator.Falloff(gradientTemp, tempA, tempB), 0,
                        tempHeightRatio * (heightMap[x, y] - seaLevel));
                }
            }

            return tempMap;
        }

        public Climate[,] GetClimateMap(float[,] heightMap, float[,] tempMap, float[,] rainMap) {
            var climateMap = new Climate[worldSize, worldSize];

            var tempTypes = climateTable.GetLength(0);
            var rainTypes = climateTable.GetLength(1);

            for (var y = 0; y < worldSize; y++) {
                for (var x = 0; x < worldSize; x++) {
                    var height = heightMap[x, y];
                    if (height < seaLevel) {
                        climateMap[x, y] = seaClimate;
                    }
                    else if (height > mountainLevel) {
                        climateMap[x, y] = mountainClimate;
                    }
                    else {
                        var temp = Mathf.Clamp(tempMap[x, y], 0f, .99f);
                        var tempIndex = Mathf.FloorToInt(temp * tempTypes);

                        var rain = Mathf.Clamp(rainMap[x, y], 0f, .99f);
                        var rainIndex = Mathf.FloorToInt(rain * rainTypes);

                        climateMap[x, y] = climateTable[tempIndex, rainIndex];
                    }
                }
            }

            return climateMap;
        }
    }

    [Serializable]
    public struct NoiseParameters {
        [Range(1, 8)] public int octaves;
        [Range(0, 1)] public float persistance;
        [Range(1, 5)] public float lacunarity;
        [Range(10, 200)] public int scale;
    }
}