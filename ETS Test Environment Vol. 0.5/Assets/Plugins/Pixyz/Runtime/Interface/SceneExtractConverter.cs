using Pixyz.Import;
using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Plugin4Unity;

namespace Pixyz.Interface {

    public sealed class UnityToSceneExtract
    {
        public NativeInterface.SceneExtract sceneExtract { get; private set; }

        public Dictionary<int, MeshToMeshDefinition> meshes = new Dictionary<int, MeshToMeshDefinition>();
        private Dictionary<int, NativeInterface.MaterialDefinition> materials = new Dictionary<int, NativeInterface.MaterialDefinition>();
        private List<int> materialRefs = new List<int>();
        private Dictionary<int, NativeInterface.ImageDefinition> images = new Dictionary<int, NativeInterface.ImageDefinition>();
        public LookupTable<int, GameObject> partsRef = new LookupTable<int, GameObject>();
        private List<int> meshesRefs = new List<int>();

        private bool manageTextures;
        private bool isLeftHanded;
        private bool isZup;
        private float scaleFactor;

        public UnityToSceneExtract(GameObject[] gameObjects, bool isLeftHanded, bool isZup, float scaleFactor, bool manageTextures) {
            this.isLeftHanded = isLeftHanded;
            this.isZup = isZup;
            this.scaleFactor = scaleFactor;
            this.manageTextures = manageTextures;
            BuildScene(gameObjects);
        }

        private void BuildScene(GameObject[] gameObjects) {

            sceneExtract = new NativeInterface.SceneExtract();

            var tree = new List<int>();
            var partsRefs = new List<int>();
            var names = new List<string>();
            var matrices = new List<NativeInterface.Matrix4>();
            var visibilites = new List<int>();
            var properties = new List<NativeInterface.Properties>();
            var metadata = new List<NativeInterface.Properties>();
            var matricesRef = new List<int>();

            tree.Add(names.Count());
            partsRefs.Add(-1);
            visibilites.Add(1);
            names.Add("Group");
            matricesRef.Add(-1);

            foreach (GameObject gameObject in gameObjects) {

                // Mesh
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                partsRefs.Add((mf) ? meshesRefs.Count : -1);
                ConvertMesh(gameObject);
                tree.Add(names.Count());
                tree.Add(-1);

                // Metadatas
                Metadata[] metadatas = new Metadata[0];
                NativeInterface.Properties data = new NativeInterface.Properties();
                data.names = new NativeInterface.StringList(0);
                data.values = new NativeInterface.StringList(0);
                metadata.Add(data);

                // Visibilities
                visibilites.Add(1);

                // Names
                names.Add(gameObject.GetInstanceID().ToString());

                // Matrices
                matricesRef.Add(matrices.Count);
                matrices.Add(gameObject.transform.GetWorldMatrix(isLeftHanded, isZup, scaleFactor).ToInterfaceObject());
            }

            tree.RemoveAt(tree.Count - 1);

            // Building scene extract
            sceneExtract.tree = new NativeInterface.IntList(tree.ToArray());
            sceneExtract.names = new NativeInterface.StringList(names.ToArray());
            sceneExtract.materialsMode = MaterialExtractMode.PART_ONLY;
            sceneExtract.matrixMode = MatrixExtractMode.GLOBAL;
            sceneExtract.partsRef = new NativeInterface.IntList(partsRefs.ToArray());
            sceneExtract.visibilityMode = VisibilityExtractMode.VISIBLE_ONLY;
            sceneExtract.meshes = new NativeInterface.MeshDefinitionList(meshes.Count());
            sceneExtract.matrices = new NativeInterface.Matrix4List(matrices.ToArray());
            sceneExtract.matricesRef = new NativeInterface.IntList(matricesRef.ToArray());
            sceneExtract.materials = new NativeInterface.MaterialDefinitionList(materials.Count());
            sceneExtract.visibilities = new NativeInterface.IntList(visibilites.ToArray());
            sceneExtract.metadata = new NativeInterface.PropertiesList(metadata.ToArray());
            sceneExtract.properties = new NativeInterface.PropertiesList(properties.ToArray());
            KeyValuePair<int, NativeInterface.MaterialDefinition>[] matPairs = materials.ToArray();
            for (int i = 0; i < matPairs.Count(); ++i) {
                sceneExtract.materials[i] = matPairs[i].Value;
                materialRefs = materialRefs.Select(x => x == matPairs[i].Key ? i : x).ToList<int>();
            }
            sceneExtract.materialsRef = new NativeInterface.IntList(materialRefs.ToArray());
            KeyValuePair<int, MeshToMeshDefinition>[] meshPairs = meshes.ToArray();
            for (int i = 0; i < meshPairs.Count(); ++i) {
                sceneExtract.meshes[i] = meshPairs[i].Value.meshDefinition;
                meshesRefs = meshesRefs.Select(x => x == meshPairs[i].Key ? i : x).ToList<int>();
            }
            sceneExtract.meshesRef = new NativeInterface.IntList(meshesRefs.ToArray());
            sceneExtract.images = new NativeInterface.ImageDefinitionList(images.Values.ToArray());
            Array.Sort(sceneExtract.images.list, delegate (NativeInterface.ImageDefinition img1, NativeInterface.ImageDefinition img2) { return img1.id.CompareTo(img2.id); });
        }

        private void ConvertMesh(GameObject gameObject)
        {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (!mf)
                return;

            meshesRefs.Add(mf.sharedMesh.GetInstanceID());
            if (!meshes.ContainsKey(mf.sharedMesh.GetInstanceID()))
            {
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                NativeInterface.MaterialDefinition[] matextr = new NativeInterface.MaterialDefinition[meshRenderer.sharedMaterials.Length];
                if (meshRenderer) {
                    // Put materials on submeshes
                    Material[] locmat = meshRenderer.sharedMaterials;
                    materialRefs.Add(-1);
                    for (int m = 0; m < locmat.Length; m++)
                    {
                        if(!materials.TryGetValue(locmat[m].GetInstanceID(), out matextr[m])) {
                            matextr[m] = manageTextures ? locmat[m].ToNative(ref images) : locmat[m].ToNativeNoTextures();
                            materials.Add(locmat[m].GetInstanceID(), matextr[m]);
                        }
                    }
                }
                var mc = new MeshToMeshDefinition();
                Dispatcher.StartCoroutine(mc.Convert(mf.sharedMesh, matextr, isLeftHanded, isZup, scaleFactor));
                partsRef.Add(mf.sharedMesh.GetInstanceID(), gameObject);   
                meshes.Add(mf.sharedMesh.GetInstanceID(), mc);
            }
            else
            {
                partsRef.Add(mf.sharedMesh.GetInstanceID(), gameObject);
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                if (mr && mf.sharedMesh.subMeshCount == 1)
                {
                    Material locmat = mr.sharedMaterial;
                    materialRefs.Add(locmat.GetInstanceID());
                    if (!materials.ContainsKey(locmat.GetInstanceID()))
                    {
                        materials.Add(locmat.GetInstanceID(), locmat.ToNative(ref images));
                    }
                } else {
                    materialRefs.Add(-1);
                }
            }
        }
    }
}