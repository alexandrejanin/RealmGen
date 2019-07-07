using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace WorldGen {
    public class WorldGenerator : SerializedMonoBehaviour {
        [SerializeField]
        private bool generateWorldOnUpdate;

        [SerializeField,
         OnValueChanged(nameof(OnParameterUpdated))]
        private int seed;

        [SerializeField]
        private WorldParameters worldParameters;

        public UnityEvent onWorldGenerated;

        [Button(ButtonSizes.Medium)]
        private void GenerateWorld() {
            World.GenerateWorld(seed, worldParameters, onWorldGenerated.Invoke, OnParameterUpdated);
        }

        private void OnParameterUpdated() {
            if (generateWorldOnUpdate)
                GenerateWorld();
        }
    }
}