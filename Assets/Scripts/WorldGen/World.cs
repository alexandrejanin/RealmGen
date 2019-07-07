using System;
using UnityEngine;

namespace WorldGen {
    public class World {
        private readonly int seed;

        public WorldParameters WorldParameters { get; }
        public float[,] HeightMap { get; }
        public float[,] TempMap { get; }
        public float[,] RainMap { get; }
        private Vector2[,] SlopeMap { get; }
        private Climate[,] ClimateMap { get; }


        public static World Current { get; private set; }

        private World(int seed, WorldParameters worldParameters, Action onParameterUpdated) {
            this.seed = seed;

            worldParameters.OnUpdateCallback = onParameterUpdated;
            WorldParameters = worldParameters;

            HeightMap = worldParameters.GetHeightMap(seed);
            SlopeMap = worldParameters.GetSlopeMap(HeightMap);
            RainMap = worldParameters.GetRainMap(2 * seed);
            TempMap = worldParameters.GetTempMap(HeightMap);
            ClimateMap = worldParameters.GetClimateMap(HeightMap, TempMap, RainMap);
        }

        public float GetHeight(int x, int y) => HeightMap[x, y];
        public Vector2 GetSlope(int x, int y) => SlopeMap[x, y];
        public Climate GetClimate(int x, int y) => ClimateMap[x, y];

        public static void GenerateWorld(
            int seed,
            WorldParameters worldParameters,
            Action onWorldGenerated,
            Action onParameterUpdated
        ) {
            Current = new World(seed, worldParameters, onParameterUpdated);
            onWorldGenerated?.Invoke();
        }
    }
}