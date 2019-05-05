using UnityEngine;

namespace WorldGen {
    public class World {
        public readonly int seed;
        public readonly WorldParameters worldParameters;
        public readonly float[,] heightMap, tempMap, rainMap;

        public readonly Vector2[,] slopeMap;

        public readonly Climate[,] climateMap;

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
    }
}