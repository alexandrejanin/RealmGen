using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WorldGen {
    [Serializable]
    public class WorldParameters : SerializedScriptableObject {
        // Size Parameters

        [Range(16, 1024),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private int width, height;

        public int Width => width;
        public int Height => height;


        // Height Parameters

        [TabGroup("Height"),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private NoiseParameters heightParameters;

        [TabGroup("Height"),
         Range(0, 10),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private float falloffA, falloffB;

        [TabGroup("Height"),
         Range(0, 1),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private float falloffMultiplier;

        [TabGroup("Height"),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private bool normalize;


        // Rain Parameters

        [TabGroup("Rain"),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private NoiseParameters rainParameters;


        // Temperature Parameters

        [TabGroup("Temperature"),
         Range(0, 10),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private float tempA, tempB;

        [TabGroup("Temperature"),
         Range(0, 1),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private float tempHeightRatio;


        // Climate Parameters

        [TabGroup("Climates"),
         Range(0, 1),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private float seaLevel = .35f, mountainLevel = .65f;

        public float SeaLevel => seaLevel;
        public float MountainLevel => mountainLevel;

        [TabGroup("Climates"),
         SerializeField,
         OnValueChanged(nameof(OnUpdate))
        ]
        private Climate seaClimate, mountainClimate;

        [TabGroup("Climates"),
         SerializeField,
         TableMatrix(HorizontalTitle = "Temperature", VerticalTitle = "Rain"),
         OnValueChanged(nameof(OnUpdate))
        ]
        private Climate[,] climateTable;


        public Action OnUpdateCallback { set; private get; }

        private void OnUpdate() => OnUpdateCallback?.Invoke();


        public float[,] GetHeightMap(int seed) {
            var heightMap = NoiseGenerator.GenerateNoiseMap(width, height, seed, heightParameters, normalize);

            var falloffMap = NoiseGenerator.GetFalloffMap(width, height, falloffA, falloffB);
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    heightMap[x, y] = Mathf.Clamp01(heightMap[x, y] - falloffMap[x, y] * falloffMultiplier);
                }
            }

            return heightMap;
        }

        public Vector2[,] GetSlopeMap(float[,] heightMap) {
            var slopeMap = new Vector2[width, height];

            for (var y = 1; y < height - 1; y++) {
                for (var x = 1; x < width - 1; x++) {
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
            return NoiseGenerator.GenerateNoiseMap(width, height, seed, rainParameters, true);
        }

        public float[,] GetTempMap(float[,] heightMap) {
            var tempMap = new float[width, height];

            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var gradientTemp = y / (float) height;
                    tempMap[x, y] = Mathf.Lerp(NoiseGenerator.Falloff(gradientTemp, tempA, tempB), 0,
                        tempHeightRatio * (heightMap[x, y] - seaLevel));
                }
            }

            return tempMap;
        }

        public Climate[,] GetClimateMap(float[,] heightMap, float[,] tempMap, float[,] rainMap) {
            var climateMap = new Climate[width, height];

            var tempTypes = climateTable.GetLength(0);
            var rainTypes = climateTable.GetLength(1);

            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
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
        [Range(1, 8)]
        public int octaves;

        [Range(0, 1)]
        public float persistance;

        [Range(1, 5)]
        public float lacunarity;

        [Range(10, 200)]
        public int scale;
    }
}