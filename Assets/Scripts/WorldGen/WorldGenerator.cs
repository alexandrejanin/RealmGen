using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace WorldGen {
	public class WorldGenerator : SerializedMonoBehaviour {
		[SerializeField] private bool generateWorldOnUpdate;

		[SerializeField, OnValueChanged(nameof(OnParametersUpdated))]
		private int seed;

		[SerializeField, OnValueChanged(nameof(OnParametersUpdated))]
		private WorldParameters worldParameters;

		public UnityEvent onWorldGenerated;

		[Button(ButtonSizes.Medium)]
		private void GenerateWorld() {
			World.GenerateWorld(seed, worldParameters);
			onWorldGenerated.Invoke();
		}

		private void OnParametersUpdated() {
			if (generateWorldOnUpdate) GenerateWorld();
		}
	}
}