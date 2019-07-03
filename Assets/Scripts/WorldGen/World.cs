using UnityEngine;

namespace WorldGen {
    public class World {
        private readonly int seed;

        private readonly WorldParameters worldParameters;
        private readonly float[,] heightMap;
        private readonly float[,] tempMap;
        private readonly float[,] rainMap;
        private readonly Vector2[,] slopeMap;
        private readonly Climate[,] climateMap;

        public WorldParameters WorldParameters => worldParameters;
        public float[,] HeightMap => heightMap;
        public float[,] TempMap => tempMap;
        public float[,] RainMap => rainMap;

        public static World Current { get; private set; }

        private World(int seed, WorldParameters worldParameters) {
            this.seed = seed;
            this.worldParameters = worldParameters;

            heightMap = worldParameters.GetHeightMap(seed);

            slopeMap = worldParameters.GetSlopeMap(heightMap);

            rainMap = worldParameters.GetRainMap(2 * seed);

            tempMap = worldParameters.GetTempMap(heightMap);

            climateMap = worldParameters.GetClimateMap(heightMap, tempMap, rainMap);
        }

        public static void GenerateWorld(int seed, WorldParameters worldParameters) {
            Current = new World(seed, worldParameters);
        }

        public float GetHeight(int x, int y) => heightMap[x, y];
        public Vector2 GetSlope(int x, int y) => slopeMap[x, y];
        public Climate GetClimate(int x, int y) => climateMap[x, y];
    }
}