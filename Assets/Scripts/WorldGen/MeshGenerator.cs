using UnityEngine;

namespace WorldGen {
    public static class MeshGenerator {
        public static Mesh GenerateTerrainMesh(
            float[,] heightMap,
            AnimationCurve heightCurve,
            float seaLevel,
            float heightMultiplier
        ) {
            var width = heightMap.GetLength(0);
            var height = heightMap.GetLength(1);

            var lod = Mathf.Max(width, height) / 256;
            var meshStepSize = lod == 0 ? 1 : lod * 2;
            
            var meshWidth = (width - 1) / meshStepSize + 1;
            var meshHeight = (height - 1) / meshStepSize + 1;

            var topLeftX = (width - 1) / -2f;
            var topLeftZ = (height - 1) / 2f;

            var meshData = new MeshData(meshWidth, meshHeight);
            var vertexIndex = 0;

            for (var y = 0; y < height; y += meshStepSize) {
                for (var x = 0; x < width; x += meshStepSize) {
                    var heightAtPoint = heightMap[x, y];
                    var vertexHeight = heightAtPoint <= seaLevel
                        ? 0
                        : heightCurve.Evaluate(Mathf.InverseLerp(seaLevel, 1f, heightAtPoint)) *
                          heightMultiplier;
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ - y);
                    meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);

                    if (x < width - 1 && y < height - 1) {
                        meshData.AddTriangle(vertexIndex, vertexIndex + meshWidth + 1, vertexIndex + meshWidth);
                        meshData.AddTriangle(vertexIndex + meshWidth + 1, vertexIndex, vertexIndex + 1);
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