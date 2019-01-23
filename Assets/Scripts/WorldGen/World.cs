using UnityEngine;

namespace WorldGen {
	public class World {
		// Public
		public readonly int seed;
		public readonly WorldParameters worldParameters;
		public readonly float[,] heightMap, tempMap, rainMap;

		public readonly Vector2[,] slopeMap;
		public readonly Vector2[,] windMap;

		public readonly Climate[,] climateMap;

		// Static
		public static World Current { get; private set; }

		// Constructor
		private World(int seed, WorldParameters worldParameters) {
			this.seed = seed;
			this.worldParameters = worldParameters;

			heightMap = worldParameters.GetHeightMap(seed);

			slopeMap = worldParameters.GetSlopeMap(heightMap);

			windMap = worldParameters.GetWindMap(heightMap, slopeMap);

			rainMap = worldParameters.GetRainMap(heightMap, windMap);

			tempMap = worldParameters.GetTempMap(heightMap);

			climateMap = worldParameters.GetClimateMap(heightMap, tempMap, rainMap);
		}

		public static void GenerateWorld(int seed, WorldParameters worldParameters) {
			Current = new World(seed, worldParameters);
		}
	}
}