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

		[TabGroup("Wind"), Range(0, 1), SerializeField]
		private float windSlopeRatio, windDirRatio;

		[TabGroup("Wind"), SerializeField] private Vector2 windDir = Vector2.right;

		[TabGroup("Temperature"), Range(0, 10), SerializeField]
		private float tempA, tempB;

		[TabGroup("Temperature"), Range(0, 1), SerializeField]
		private float tempHeightRatio;

		[TabGroup("Rain"), SerializeField] private float windRainRatio;

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

		public Vector2[,] GetWindMap(float[,] heightMap, Vector2[,] slopeMap) {
			if (windDir.magnitude < .1f) {
				Debug.LogError("windDir too low!");
				return null;
			}

			var windMap = new Vector2[worldSize, worldSize];

			for (var startY = 0; startY < worldSize; startY++) {
				var pos = new Vector2(0, startY);
				var dir = windDir;

				var lastX = -1;
				var lastY = -1;

				do {
					var x = (int) pos.x;
					var y = (int) pos.y;

					// Stayed on same tile
					if (x == lastX && y == lastY) {
						windMap[x, y] -= dir;
					}

					//TODO: merge winds?

					// Follow slopes
					var dirChange = windSlopeRatio * heightMap[x, y] * slopeMap[x, y];
					dir += dirChange;

					// Go towards default wind direction
					var magnitude = dir.magnitude;
					dir = Vector2.Lerp(dir, windDir, windDirRatio);
					dir = magnitude * dir.normalized;

					if (dir.magnitude > 1f) dir.Normalize();

					windMap[x, y] += dir;
					pos += dir;

					lastX = x;
					lastY = y;
				} while (pos.x >= 0 && pos.x < worldSize && pos.y >= 0 && pos.y < worldSize && dir.magnitude > .01f);
			}

			return windMap;
		}

		public float[,] GetRainMap(float[,] heightMap, Vector2[,] windMap) {
			var rainMap = new float[worldSize, worldSize];

			return NoiseGenerator.Blur(rainMap);
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