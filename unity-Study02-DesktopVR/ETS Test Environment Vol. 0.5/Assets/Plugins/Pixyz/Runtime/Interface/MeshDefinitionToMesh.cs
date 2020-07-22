using Pixyz.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Plugin4Unity;

namespace Pixyz.Interface {

    public delegate void MeshDefinitionToMeshHandler(MeshDefinitionToMesh meshDefinitionConverter);

    /// <summary>
    /// Async !!!
    /// </summary>
    public sealed class MeshDefinitionToMesh {

        public Mesh mesh;

        public NativeInterface.MeshDefinition meshDefinition;

        private Color[] vertexColors;
        private int[] pointsInd;
        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector4[] tangents;
        private int[][] triangles;
        private Vector2[][] uvsc;
        private List<MeshTopology> topologies;

        private List<int[]> edges;

        private MeshDefinitionToMeshHandler conversionCallback;

        public int subMeshCount => topologies.Count;
        public bool isCompleted { get; private set; }

        public int polyCount => _polyCount;
        public int _polyCount;

        public uint getNativeMaterial(int submesh) {
            if (submesh > meshDefinition.dressedPolys.length - 1)
                return 0;
            return meshDefinition.dressedPolys[submesh].material;
        }

        public Color getNativeLineColor(int submesh) {
            if (submesh < subMeshCount - meshDefinition.lines.length)
                return Color.black;
            NativeInterface.ColorAlpha color = meshDefinition.lines[submesh - (subMeshCount - meshDefinition.lines.length)].color;
            return new Color((float)color.r, (float)color.g, (float)color.b, (float)color.a);
        }

        public MeshTopology getSubmeshTopology(int i) {
            return topologies[i];
        }

        public IEnumerator Convert(NativeInterface.MeshDefinition meshDefinition, bool isLeftHanded, bool isZup, float scale, MeshDefinitionToMeshHandler conversionCallback = null, bool isAsynchronous = false) {

            // This is required ! The Core doesn't keep dressedPoly order 
            meshDefinition.dressedPolys.list = meshDefinition.dressedPolys.list.OrderBy(x => x.externalId).ToArray();

            edges = new List<int[]>();
            triangles = new int[meshDefinition.dressedPolys.length][];
            topologies = new List<MeshTopology>();

            this.conversionCallback = conversionCallback;
            this.meshDefinition = meshDefinition;

            // Unity is LeftHanded. If model is Right Handed, we change model handedness.
            var flipX = (isLeftHanded ? 1 : -1);

            if (meshDefinition.linesVertices.length == 0 && meshDefinition.vertices.length == 0 && meshDefinition.points.length == 0) {
                complete();
                yield break;
            }

            for (int i = 0; i < triangles.Length; i++) {
                topologies.Add(MeshTopology.Triangles);
            }

            for (int i = 0; i < meshDefinition.lines.length; i++) {
                topologies.Add(MeshTopology.Lines);
            }

            if (meshDefinition.points.length > 0) {
                topologies.Add(MeshTopology.Points);
            }

            mesh = new Mesh();

            // Adds the mesh data preparation work in a thread (managed by ThreadPool).
            // When the prepateMeshData is over, the main thread will then be able run setMeshData() on the next frame.
            if (isAsynchronous)
                yield return Dispatcher.GoThreadPool();

            try {
                // Convert Vertices
                vertices = new Vector3[meshDefinition.vertices.length + meshDefinition.linesVertices.length + meshDefinition.points.length];
                {
                    int offset = 0;
                    // Convert Mesh Vertices
                    if (isZup) {
                        for (int i = 0; i < meshDefinition.vertices.length; i++) {
                            vertices[i] = new Vector3((float)meshDefinition.vertices[i].x * scale * flipX, (float)meshDefinition.vertices[i].z * scale, (float)meshDefinition.vertices[i].y * scale);
                        }
                    } else {
                        for (int i = 0; i < meshDefinition.vertices.length; i++) {
                            vertices[i] = new Vector3((float)meshDefinition.vertices[i].x * scale * flipX, (float)meshDefinition.vertices[i].y * scale, (float)meshDefinition.vertices[i].z * scale);
                        }
                    }
                    offset += meshDefinition.vertices.length;
                    // Convert Lines Vertices
                    if (isZup) {
                        for (int i = 0; i < meshDefinition.linesVertices.length; i++) {
                            vertices[i + offset] = new Vector3((float)meshDefinition.linesVertices[i].x * scale * flipX, (float)meshDefinition.linesVertices[i].z * scale, (float)meshDefinition.linesVertices[i].y * scale);
                        }
                    } else {
                        for (int i = 0; i < meshDefinition.linesVertices.length; i++) {
                            vertices[i + offset] = new Vector3((float)meshDefinition.linesVertices[i].x * scale * flipX, (float)meshDefinition.linesVertices[i].y * scale, (float)meshDefinition.linesVertices[i].z * scale);
                        }
                    }
                    offset += meshDefinition.linesVertices.length;
                    // Convert Free Vertices
                    if (isZup) {
                        for (int i = 0; i < meshDefinition.points.length; i++) {
                            vertices[i + offset] = new Vector3((float)meshDefinition.points[i].x * scale * flipX, (float)meshDefinition.points[i].z * scale, (float)meshDefinition.points[i].y * scale);
                        }
                    } else {
                        for (int i = 0; i < meshDefinition.points.length; i++) {
                            vertices[i + offset] = new Vector3((float)meshDefinition.points[i].x * scale * flipX, (float)meshDefinition.points[i].y * scale, (float)meshDefinition.points[i].z * scale);
                        }
                    }
                }

                // Convert Vertex Colors
                vertexColors = new Color[meshDefinition.vertices.length + meshDefinition.linesVertices.length + meshDefinition.points.length];
                {
                    int offset = 0;
                    // Convert Mesh Vertex Colors
                    for (int i = 0; i < meshDefinition.vertexColors.length; i++) {
                        NativeInterface.ColorAlpha color = meshDefinition.vertexColors[i];
                        vertexColors[i] = new Color((float)color.r, (float)color.g, (float)color.b, (float)color.a);
                    }
                    offset += meshDefinition.vertices.length;
                    // Convert Lines Vertex Colors
                    /// No code here (there are not such 'line vertex color yet')
                    offset += meshDefinition.linesVertices.length;
                    // Convert Free Vertex Colors
                    for (int i = 0; i < meshDefinition.pointsColors.length; i++) {
                        NativeInterface.Point3 color = meshDefinition.pointsColors[i];
                        vertexColors[i + offset] = new Color((float)color.x, (float)color.y, (float)color.z, 1f);
                    }
                }

                // Convert Triangles Indices
                for (int s = 0; s < triangles.Length; s++) {
                    int offset = meshDefinition.dressedPolys[s].firstTri * 3;
                    int trcount = meshDefinition.dressedPolys[s].triCount * 3;
                    var striangles = triangles[s] = new int[trcount];
                    if (isLeftHanded ^ !isZup) {
                        for (int t = 0; t < trcount; t += 3) {
                            striangles[t + 0] = meshDefinition.triangles[offset + t + 1];
                            striangles[t + 1] = meshDefinition.triangles[offset + t + 0];
                            striangles[t + 2] = meshDefinition.triangles[offset + t + 2];
                        }
                    } else {
                        for (int t = 0; t < trcount; t += 3) {
                            striangles[t + 0] = meshDefinition.triangles[offset + t + 0];
                            striangles[t + 1] = meshDefinition.triangles[offset + t + 1];
                            striangles[t + 2] = meshDefinition.triangles[offset + t + 2];
                        }
                    }
                }

                // Convert Lines Indices
                foreach (NativeInterface.StylizedLine line in meshDefinition.lines.list) {
                    NativeInterface.IntList indices = line.lines;
                    if (meshDefinition.vertices.length != 0) {
                        for (int i = 0; i < line.lines.length; ++i)
                            indices[i] += meshDefinition.vertices.length;
                    }
                    edges.Add(indices);
                }

                // Convert Points indices
                pointsInd = new int[meshDefinition.points.length];
                for (int i = 0; i < pointsInd.Length; i++) {
                    pointsInd[i] = vertices.Length - meshDefinition.points.length + i;
                }

                // Convert Normals
                int normalsCount = meshDefinition.normals.length + ((meshDefinition.normals.length == 0) ? 0 : meshDefinition.linesVertices.length);
                normals = new Vector3[normalsCount];
                if (isZup) {
                    for (int i = 0; i < meshDefinition.normals.length; i++) {
                        NativeInterface.Point3 point = meshDefinition.normals[i];
                        normals[i] = new Vector3((float)point.x * flipX, (float)point.z, (float)point.y);
                    }
                } else {
                    for (int i = 0; i < meshDefinition.normals.length; i++) {
                        NativeInterface.Point3 point = meshDefinition.normals[i];
                        normals[i] = new Vector3((float)point.x * flipX, (float)point.y, (float)point.z);
                    }
                }

                // Convert Tangents
                int tangentsCount = meshDefinition.tangents.length + ((meshDefinition.tangents.length == 0) ? 0 : meshDefinition.linesVertices.length);
                tangents = new Vector4[tangentsCount];
                if (isZup) {
                    for (int i = 0; i < meshDefinition.tangents.length; i++) {
                        NativeInterface.Point3 point = meshDefinition.tangents[i];
                        tangents[i] = new Vector4((float)point.x * flipX, (float)point.z, (float)point.y, 1f);
                    }
                } else {
                    for (int i = 0; i < meshDefinition.tangents.length; i++) {
                        NativeInterface.Point3 point = meshDefinition.tangents[i];
                        tangents[i] = new Vector4((float)point.x * flipX, (float)point.y, (float)point.z, 1f);
                    }
                }

                // Convert UVs
                uvsc = new Vector2[meshDefinition.uvChannels.length][];
                for (int j = 0; j < uvsc.Length; j++) {
                    // What is dat ?
                    if (meshDefinition.uvChannels[j] < meshDefinition.uvs.length) {
                        NativeInterface.Point2List iuvs = meshDefinition.uvs[meshDefinition.uvChannels[j]];
                        Vector2[] uvsj = uvsc[j] = new Vector2[vertices.Length];
                        int uvLength = iuvs.length;
                        for (int i = 0; i < uvsj.Length; i++) {
                            if (i < uvLength)
                                uvsj[i] = new Vector2((float)iuvs[i].x, (float)iuvs[i].y);
                            else {
                                // To ensure that we have uv size = vertices size.
                                // This is useful when having indices without uvs such as lines.
                                // This only works if vertices without uvs are at the end of the vertex list.
                                uvsj[i] = Vector3.zero;
                            }
                        }
                    }
                }
            } catch (Exception exception) {
                Debug.LogError("MeshDefinitionConverter.prepareMeshData() : " + exception);
                complete();
                yield break;
            }

            if (isAsynchronous)
                yield return Dispatcher.GoMainThread();

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            // Assign Vertices
            mesh.vertices = vertices;

            // Assign UVs
            for (int j = 0; j < uvsc.Length; j++) {
                try {
                    switch (j) {
                        case -1:
                            // Ignore
                            break;
                        case 0:
                            mesh.uv = uvsc[j];
                            break;
                        case 1:
                            mesh.uv2 = uvsc[j];
                            break;
                        case 2:
                            mesh.uv3 = uvsc[j];
                            break;
                        case 3:
                            mesh.uv4 = uvsc[j];
                            break;
                        case 4:
                            mesh.uv5 = uvsc[j];
                            break;
                        case 5:
                            mesh.uv6 = uvsc[j];
                            break;
                        case 6:
                            mesh.uv7 = uvsc[j];
                            break;
                        case 7:
                            mesh.uv8 = uvsc[j];
                            break;
                        default:
                            Debug.LogError("Mesh doesn't have an UV set n°" + j);
                            break;
                    }
                } catch (Exception exception) {
                    Debug.LogError(exception);
                }
            }

            // Assign Normals
            mesh.normals = normals;

            // Assign Tangents
            mesh.tangents = tangents;

            // Assign Vertex Colors
            mesh.colors = vertexColors;

            var triangleSubmeshes = new List<int>();
            var linesSubmeshes = new List<int>();
            var pointsSubmeshes = new List<int>();
            for (int i = 0; i < topologies.Count; i++) {
                if (topologies[i] == MeshTopology.Triangles) {
                    triangleSubmeshes.Add(i);
                } else if (topologies[i] == MeshTopology.Lines) {
                    linesSubmeshes.Add(i);
                } else {
                    pointsSubmeshes.Add(i);
                }
            }

            mesh.subMeshCount = subMeshCount;

            // Assign Triangles Indices
            for (int s = 0; s < triangleSubmeshes.Count; s++) {
                mesh.SetTriangles(triangles[s], triangleSubmeshes[s]);
            }

            // Assign Free Vertices Indices
            if (pointsSubmeshes.Count > 0) {
                mesh.SetIndices(pointsInd, MeshTopology.Points, pointsSubmeshes[0], false);
            }

            // Assign Lines Indices
            for (int s = 0; s < linesSubmeshes.Count; s++) {
                mesh.SetIndices(edges[s], MeshTopology.Lines, linesSubmeshes[s]);
            }

            ; // Optimize vertex cache
            mesh.RecalculateBounds();

            complete();
        }

        private void complete() {
            isCompleted = true;
            conversionCallback?.Invoke(this);
        }
    }
}