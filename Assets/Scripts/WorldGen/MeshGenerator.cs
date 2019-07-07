using UnityEngine;

namespace WorldGen {
    public static class MeshGenerator {
        public static Mesh[,] GenerateTerrainMesh(
            float[,] heightMap,
            AnimationCurve heightCurve,
            float seaLevel,
            float heightMultiplier
        ) {
            var width = heightMap.GetLength(0);
            var height = heightMap.GetLength(1);

            var topLeftX = (width - 1) / -2f;
            var topLeftZ = (height - 1) / 2f;

            var chunks = new[,] {{new MeshData(width, height)}};
            var vertexIndex = 0;

            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var heightAtPoint = heightMap[x, y];
                    var vertexHeight = heightAtPoint <= seaLevel
                        ? 0
                        : heightCurve.Evaluate(Mathf.InverseLerp(seaLevel, 1f, heightAtPoint)) *
                          heightMultiplier;
                    chunks[0, 0].vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ - y);
                    chunks[0, 0].uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);

                    if (x < width - 1 && y < height - 1) {
                        chunks[0, 0].AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                        chunks[0, 0].AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                    }

                    vertexIndex++;
                }
            }

            var meshes = new Mesh[chunks.GetLength(0), chunks.GetLength(1)];
            for (var i = 0; i < chunks.GetLength(0); i++) {
                for (var j = 0; j < chunks.GetLength(1); j++) {
                    meshes[i, j] = chunks[i, j].CreateMesh();
                }
            }

            return meshes;
        }

        private struct MeshData {
            public readonly Vector3[] vertices;
            private readonly int[] triangles;
            public readonly Vector2[] uvs;

            private int triangleIndex;

            public MeshData(int width, int height) {
                vertices = new Vector3[width * height];
                uvs = new Vector2[width * height];
                triangles = new int[(width - 1) * (height - 1) * 6];
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