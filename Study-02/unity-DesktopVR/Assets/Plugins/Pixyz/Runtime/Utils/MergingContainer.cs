using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Utils  {

    public sealed class MergingContainer {

        const int SIZE_INIT = 1024;

        private List<Vector3> _vertices = new List<Vector3>(SIZE_INIT);
        private List<Vector3> _normals = new List<Vector3>(SIZE_INIT);
        private List<Color> _colors = new List<Color>(SIZE_INIT);
        private List<Vector4> _tangents = new List<Vector4>(SIZE_INIT);
        private List<IndicesAndTopology> _indices = new List<IndicesAndTopology>();
        internal List<List<Vector2>> _uvs = new List<List<Vector2>>();
        private Dictionary<Material, int> _materials = new Dictionary<Material, int>();

        private string name;

        public int vertexCount => _vertices.Count;

        public void add(Mesh mesh, Material[] materials, Matrix4x4 matrix) {

            if (mesh.subMeshCount != materials.Length) {
                throw new System.Exception($"The mesh '{mesh.name}' can't be merged because the number of given materials is different than the submesh count.");
            }

            if (string.IsNullOrEmpty(name))
                name = mesh.name + " merged";

            Matrix4x4 matrixTransposeInverse = matrix.transpose.inverse;

            bool isSymmetry = matrix.determinant < 0;

            // Add indices
            for (int s = 0; s < mesh.subMeshCount; s++) {
                if (materials[s] != null) {
                    var indices = mesh.GetIndices(s, true);
                    addIndices(indices, getSubmeshFromMaterial(materials[s]), verticesCount, mesh.GetTopology(s), isSymmetry);
                }
                // May crash if submesh count != material count
            }

            // Add vertices
            var vertices = mesh.vertices;
            if (matrix != Matrix4x4.identity) {
                for (int v = 0; v < vertices.Length; v++) {
                    // Apply translation, rotation & scale
                    vertices[v] = matrix.MultiplyPoint3x4(vertices[v]);
                }
            }
            addVertices(vertices);

            // Add colors
            addColors(mesh.colors);

            // Add normals
            var normals = mesh.normals;
            if (normals.Length != vertices.Length) {
                normals = new Vector3[vertices.Length];
                areNormalsConsistent = false;
            } else {
                for (int v = 0; v < normals.Length; v++) {
                    // Apply rotation & scale
                    normals[v] = matrixTransposeInverse.MultiplyVector(normals[v]);
                    normals[v].Normalize();
                }
            }
            addNormals(normals);

            // Add tangents
            var tangents = mesh.tangents;
            if (tangents.Length != vertices.Length) {
                tangents = new Vector4[vertices.Length];
            } else {
                for (int v = 0; v < tangents.Length; v++) {
                    // Apply rotation & scale
                    tangents[v] = matrixTransposeInverse.MultiplyVector(tangents[v]);
                    tangents[v].Normalize();
                }
            }
            addTangents(tangents);

            // Add UVs
            for (int c = 0; c < 8; c++) {
                var uvs = new List<Vector2>();
                mesh.GetUVs(c, uvs);

                if (uvs.Count != vertices.Length) {
                    if (_uvs.Count > c && _uvs[c].Count != 0) {
                        addUVs(new Vector2[vertices.Length], c);
                    }
                } else {
                    if (_uvs.Count <= c || _uvs[c].Count == 0) {
                        addUVs(new Vector2[verticesCount - vertices.Length], c);
                    }
                    addUVs(uvs, c);
                    // May crash if one mesh doesn't have uvs while another one after has
                }
            }
        }

        public void addGameObject(GameObject gameObject, Transform targetTransform) {

            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

            if (!mf || !mr)
                return;

            var mesh = mf.sharedMesh;
            var materials = mr.sharedMaterials;

            Matrix4x4 A = targetTransform.localToWorldMatrix;
            Matrix4x4 B = gameObject.transform.localToWorldMatrix;

            Matrix4x4 matrix = A.inverse * B;

            add(mesh, materials, matrix);
        }

        public void addGameObject(GameObject gameObject) {

            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

            if (!mf || !mr)
                return;

            var mesh = mf.sharedMesh;
            var materials = mr.sharedMaterials;

            add(mesh, materials, Matrix4x4.identity);
        }

        public void addVertices(IList<Vector3> vertices) {
            _vertices.AddRange(vertices);
        }

        public void addNormals(IList<Vector3> normals) {
            _normals.AddRange(normals);
        }

        public void addColors(IList<Color> colors) {
            _colors.AddRange(colors);
        }

        public void addTangents(IList<Vector4> tangents) {
            _tangents.AddRange(tangents);
        }

        public void addIndices(IList<int> indices, int submesh, int baseVertex, MeshTopology meshTopology, bool flip = false) {
            while (_indices.Count <= submesh) {
                _indices.Add(new IndicesAndTopology(SIZE_INIT, meshTopology));
            }
            for (int i = 0; i < indices.Count; i++) {
                indices[i] += baseVertex;
            }
            if (flip) {
                if (meshTopology == MeshTopology.Triangles) {
                    for (int i = 0; i < indices.Count; i += 3) {
                        int swap = indices[i + 1];
                        indices[i + 1] = indices[i + 2];
                        indices[i + 2] = swap;
                    }
                } else if (meshTopology == MeshTopology.Quads) {
                    for (int i = 0; i < indices.Count; i += 4) {
                        int swap = indices[i + 1];
                        indices[i + 1] = indices[i + 2];
                        indices[i + 2] = swap;
                    }
                }
            }
            _indices[submesh].indices.AddRange(indices);
        }

        public void addUVs(IList<Vector2> uvs, int channel) {
            while (_uvs.Count <= channel) {
                _uvs.Add(new List<Vector2>(SIZE_INIT));
            }
            _uvs[channel].AddRange(uvs);
        }

        public int getSubmeshFromMaterial(Material material) {
            if (_materials.ContainsKey(material))
                return _materials[material];
            else {
                int submesh = _materials.Count;
                _materials.Add(material, submesh);
                return submesh;
            }
        }

        public int submeshCount => _indices.Count;

        public int verticesCount => _vertices.Count;

        public Material[] sharedMaterials => _materials.Keys.ToArray();

        public bool areNormalsConsistent = true;

        public Mesh getMesh() {

            Mesh mesh = new Mesh();

            if (string.IsNullOrEmpty(name))
                name = "Mesh " + mesh.GetInstanceID();

            mesh.name = name;
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetColors(_colors);
            mesh.SetTangents(_tangents);
            mesh.subMeshCount = _indices.Count;

            bool hasOnlyTriangles = true;

            // Triangles
            for (int s = 0; s < _indices.Count; s++) {
                mesh.SetIndices(_indices[s].indices.ToArray(), _indices[s].meshTopology, s, true/*, baseVertices[s]*/);
                if (_indices[s].meshTopology != MeshTopology.Triangles) {
                    hasOnlyTriangles = false;
                }
            }

            // UVs
            for (int c = 0; c < _uvs.Count; c++) {
                mesh.SetUVs(c, _uvs[c]);
            }

            if (!areNormalsConsistent && hasOnlyTriangles) {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();

            // Cleanup
            _vertices = null;
            _normals = null;
            _tangents = null;
            _indices = null;

            return mesh;
        }

        private struct IndicesAndTopology
        {
            public List<int> indices;
            public MeshTopology meshTopology;

            public IndicesAndTopology(int sizeInit, MeshTopology meshTopology) {
                indices = new List<int>(sizeInit);
                this.meshTopology = meshTopology;
            }
        }
    }
}