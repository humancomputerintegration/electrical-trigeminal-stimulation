using Pixyz.Import;
using Pixyz.Interface;
using Pixyz.Plugin4Unity;
using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Pixyz.Tools.Editor {

    public sealed class CoreContext : IDisposable {

        const bool IS_CORE_LEFT_HANDED = false;
        const bool IS_CORE_Z_UP = false;
        const float UNITY_TO_CORE_SCALE_FACTOR = 1000f;

        public readonly bool manageTextures;

        public readonly ReadOnlyCollection<GameObject> gameObjects;

        public CoreContext(GameObject[] gameObjects, bool manageTextures) {

            NativeInterface.SetCurrentThreadAsMain();

            this.gameObjects = new ReadOnlyCollection<GameObject>(gameObjects);
            this.manageTextures = manageTextures;
            // Pull data to core
            NativeInterface.ClearScene();
            var sceneExtractConverter = new UnityToSceneExtract(gameObjects, IS_CORE_LEFT_HANDED, IS_CORE_Z_UP, UNITY_TO_CORE_SCALE_FACTOR, manageTextures);
            NativeInterface.SceneExtract sceneExtract = sceneExtractConverter.sceneExtract;
            NativeInterface.CreateSceneFromExtract(sceneExtract);
        }

        public void Dispose() {

            var newScene = NativeInterface.GetSceneExtract(NativeInterface.GetSceneRoot(), true, true, true, MatrixExtractMode.LOCAL, MaterialExtractMode.PART_ONLY, VisibilityExtractMode.VISIBLE_ONLY);
            NativeInterface.ClearScene();

            var meshIdToMesh = new Dictionary<int, MeshDefinitionToMesh>();
            var matIdToMaterial = new Dictionary<uint, Material>();
            var textures = new List<Texture2D>(); // Should be dictionnary <id / texture>, but there is something wrong in the SceneExtract form, it's based on order

            if (manageTextures) {
                foreach (var imageDefinition in newScene.images.list) {
                    Texture2D texture = imageDefinition.ToUnityObject();
                    textures.Add(texture);
                }
            }

            for (int i = 0; i < newScene.materials.length; i++) {
                matIdToMaterial.Add(newScene.materials[i].id, newScene.materials[i].ToUnityObject(SceneExtensions.GetDefaultShader(), textures));
            }

            var instanceIDtoGameObject = new Dictionary<int, GameObject>();

            foreach (GameObject gameObject in gameObjects) {
                instanceIDtoGameObject.Add(gameObject.GetInstanceID(), gameObject);
            }

            for (int treeIndex = 0; treeIndex < newScene.tree.length; ++treeIndex) {

                int partIndex = newScene.tree[treeIndex];
                int gameObjectId;

                if (partIndex < 0)
                    continue;

                // Retreive the original gameObject instance ID
                GameObject gameObject = null;
                bool isNameId = int.TryParse(newScene.names[partIndex], out gameObjectId);
                if (isNameId) {
                    gameObject = instanceIDtoGameObject[gameObjectId];
                    instanceIDtoGameObject.Remove(gameObjectId);
                }

                int partRef = newScene.partsRef[partIndex];
                if (partRef < 0)
                    continue;

                if (!isNameId)
                    throw new Exception($"Part name should be int : {newScene.names[partIndex]}");

                int meshId = newScene.meshesRef[partRef];

                MeshDefinitionToMesh meshConverter;
                if (meshIdToMesh.ContainsKey(meshId)) {
                    meshConverter = meshIdToMesh[meshId];
                } else {
                    meshConverter = new MeshDefinitionToMesh();
                    Dispatcher.StartCoroutine(meshConverter.Convert(newScene.meshes[meshId], IS_CORE_LEFT_HANDED, IS_CORE_Z_UP, 1f / UNITY_TO_CORE_SCALE_FACTOR, null, false));
                    meshIdToMesh.Add(meshId, meshConverter);
                }

                MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer) {
                    // Materials
                    // For now this won't manage lines, but as of today no function will manage lines + change materials
                    if (manageTextures) {
                        int matRef = newScene.materialsRef[partRef];
                        Material[] materials = new Material[meshConverter.subMeshCount];
                        for (int i = 0; i < materials.Length; ++i) {
                            if (meshConverter.getSubmeshTopology(i) == MeshTopology.Lines) {
                                //materials[i] = getLineMaterial(meshConverter.getNativeLineColor(i));
                                Debug.LogWarning("Lines are not handled when materials are modified");
                            } else {
                                materials[i] = matIdToMaterial[(matRef < 0) ? meshConverter.getNativeMaterial(i) : newScene.materials[matRef].id];
                            }
                        }
                        renderer.sharedMaterials = materials;
                    } else {
                        // Materials may need to be resized (without changing actual materials or their references) if some submeshes where removed by the Core process
                        Material[] materials = renderer.sharedMaterials;
                        int previousExternalId = -1;
                        int dressedPolyIndex = 0;
                        for (int s = 0; s < meshConverter.subMeshCount; s++) {
                            if (meshConverter.getSubmeshTopology(s) == MeshTopology.Triangles) {
                                uint currentExternalId = meshConverter.meshDefinition.dressedPolys[dressedPolyIndex].externalId;
                                for (int d = previousExternalId + 1; d < currentExternalId; d++) {
                                    CollectionExtensions.RemoveAt(ref materials, s);
                                }
                                previousExternalId = (int)currentExternalId;
                                dressedPolyIndex++;
                            }
                        }
                        Array.Resize(ref materials, meshConverter.subMeshCount);
                        renderer.sharedMaterials = materials;
                    }

                    MeshFilter filter = gameObject.GetComponent<MeshFilter>();
                    if (filter) {
                        if (meshConverter.mesh) {
                            meshConverter.mesh.name = "PXZ_" + meshConverter.mesh.GetInstanceID();//(filter.sharedMesh == null) ? "mesh" : filter.sharedMesh.name;
                            filter.sharedMesh = meshConverter.mesh;
                        } else {
                            // Mesh has been deleted in the Pixyz Context, so we delete filter and renderer in Unity
                            GameObject.DestroyImmediate(filter);
                            GameObject.DestroyImmediate(renderer);
                        }
                    }
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}