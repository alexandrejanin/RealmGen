using UnityEngine;

namespace WorldGen {
    public static class MeshGenerator {
        public static Mesh GenerateTerrainMesh(float[,] heightMap, AnimationCurve heightCurve, float seaLevel,
                                               float heightMultiplier) {
            //TODO: Allow arbitrary size maps, Allow non-square maps
            var size = heightMap.GetLength(0);

            var lod = size / 256;
            var meshStepSize = lod == 0 ? 1 : lod * 2;
            var meshSize = (size - 1) / meshStepSize + 1;

            var topLeftX = (size - 1) / -2f;
            var topLeftZ = (size - 1) / 2f;

            var meshData = new MeshData(meshSize);
            var vertexIndex = 0;

            for (var y = 0; y < size; y += meshStepSize) {
                for (var x = 0; x < size; x += meshStepSize) {
                    var height = heightMap[x, y];
                    var vertexHeight = height <= seaLevel
                        ? 0
                        : heightCurve.Evaluate(Mathf.InverseLerp(seaLevel, 1f, height)) *
                          heightMultiplier;
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ - y);
                    meshData.uvs[vertexIndex] = new Vector2(x / (float) size, y / (float) size);

                    if (x < size - 1 && y < size - 1) {
                        meshData.AddTriangle(vertexIndex, vertexIndex + meshSize + 1, vertexIndex + meshSize);
                        meshData.AddTriangle(vertexIndex + meshSize + 1, vertexIndex, vertexIndex + 1);
                    }

                    vertexIndex++;
                }
            }

            return meshData.CreateMesh();
        }

        private struct MeshData {
            public readonly Vector3[] vertices;
            private readonly int[] triangles;
            public readonly Vector2[] uvs;

            private int triangleIndex;

            public MeshData(int size) {
                vertices = new Vector3[size * size];
                uvs = new Vector2[size * size];
                triangles = new int[(size - 1) * (size - 1) * 6];
                triangleIndex = 0;
            }

            public void AddTriangle(int a, int b, int c) {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }

            public Mesh CreateMesh() {
                var mesh = new Mesh {
                    vertices = vertices,
                    triangles = triangles,
                    uv = uvs
                };
                mesh.RecalculateNormals();
                return mesh;
            }
        }
    }
}