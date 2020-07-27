using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace Pixyz.Utils  {

    public static class SceneExtensions {

        /// <summary>
        /// Returns an array of unique meshes from given GameObjects.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Mesh[] GetMeshesUnique(this IList<GameObject> input) {
            var meshes = new HashSet<Mesh>();
            for (int i = 0; i < input.Count; i++) {
                MeshFilter meshFilter = input[i].GetComponent<MeshFilter>();
                if (!meshFilter)
                    continue;
                meshes.Add(meshFilter.sharedMesh);
            }
            return meshes.ToArray();
        }

        public static Mesh[] GetMeshes(this IList<GameObject> input)
        {
            var meshes = new List<Mesh>();
            for (int i = 0; i < input.Count; i++) {
                MeshFilter meshFilter = input[i].GetComponent<MeshFilter>();
                if (!meshFilter)
                    continue;
                meshes.Add(meshFilter.sharedMesh);
            }
            return meshes.ToArray();
        }

        /// <summary>
        /// Returns an array of renderers from given GameObjects.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Renderer[] GetRenderers(this IList<GameObject> input) {
            var renderers = new List<Renderer>();
            for (int i = 0; i < input.Count; i++) {
                Renderer renderer = input[i].GetComponent<Renderer>();
                if (!renderer)
                    continue;
                renderers.Add(renderer);
            }
            return renderers.ToArray();
        }

        /// <summary>
        /// Returns bounds in world space
        /// </summary>
        /// <param name="gameObject">Object to take bounds from</param>
        /// <returns></returns>
        public static Bounds GetBoundsWorldSpace(this GameObject gameObject, bool includeChildren) {
            if (includeChildren) {
                List<GameObject> gameObjects = gameObject.GetChildren(true, true);
                return gameObjects.GetBoundsWorldSpace();
            } else {
                return new GameObject[] { gameObject }.GetBoundsWorldSpace();
            }
        }

        /// <summary>
        /// Returns bounds in world space
        /// </summary>
        /// <param name="gameObject">Objects to take bounds from</param>
        /// <returns></returns>
        public static Bounds GetBoundsWorldSpace(this IList<GameObject> gameObjects) {
            var renderers = gameObjects.GetRenderers();
            if (renderers.Length == 0)
                throw new Exception("This requires at least one Renderer !");

            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) {
                // not sure if it really works with complex transformations...
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        /// <summary>
        /// Returns an array of unique materials from given GameObjects
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Material[] GetMaterials(this IList<GameObject> input) {
            HashSet<Material> materials = new HashSet<Material>();
            for (int i = 0; i < input.Count; i++) {
                MeshRenderer meshRenderer = input[i].GetComponent<MeshRenderer>();
                if (!meshRenderer)
                    continue;
                Material[] meshMaterials = meshRenderer.sharedMaterials;
                for (int m = 0; m < meshMaterials.Length; m++) {
                    materials.Add(meshMaterials[m]);
                }
            }
            return materials.ToArray();
        }

        /// <summary>
        /// Merge children (recursively) of given GameObject.
        /// If given GameObject has no mesh or renderer, this method will create it with the merged content inside.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void MergeChildren(this GameObject gameObject) {

            Regex regex = new Regex("_LOD[1-9]$");

            MergingContainer meshTransfer = new MergingContainer();
            meshTransfer.addGameObject(gameObject);
            var gameObjects = gameObject.GetChildren(true, false);
            for (int i = 0; i < gameObjects.Count; i++) {
                if (!regex.IsMatch(gameObjects[i].name)) { // Don't merge LODs lower than 0
                    meshTransfer.addGameObject(gameObjects[i], gameObject.transform);
                }
            }

            if (meshTransfer.vertexCount > 0) {
                gameObject.GetOrAddComponent<MeshFilter>().sharedMesh = meshTransfer.getMesh();
                gameObject.GetOrAddComponent<MeshRenderer>().sharedMaterials = meshTransfer.sharedMaterials;
            }

            for (int i = 0; i < gameObjects.Count; i++) {
                GameObject.DestroyImmediate(gameObjects[i]);
            }
        }

        public static Mesh GetMesh(this GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>(); ;
            if (!meshFilter)
                return null;

            Mesh mesh = meshFilter.sharedMesh;
            if (!mesh)
                return null;

            return mesh;
        }

        /// <summary>
        /// Explodes submeshes from a given GameObject into multiple child GameObjects.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="firstSubmeshAsParent">If true, the first submesh will stay in the gameObject's mesh. Otherwise, one child GameObject is created per submesh and the original meshfilter and renderers gets destroyed.</param>
        /// <returns></returns>
        public static List<GameObject> ExplodeSubmeshes(this GameObject gameObject, bool firstSubmeshAsParent) {

            List<GameObject> newGameObjects = new List<GameObject>();

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>(); ;
            if (!meshFilter)
                return newGameObjects;

            Mesh mesh = meshFilter.sharedMesh;
            if (!mesh)
                return newGameObjects;

            Material[] materials;
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer) {
                materials = renderer.sharedMaterials;
                Array.Resize(ref materials, mesh.subMeshCount);
            } else {
                materials = new Material[mesh.subMeshCount];
            }

            if (firstSubmeshAsParent) {
                for (int s = 0; s < mesh.subMeshCount; s++) {
                    Mesh submesh = mesh.GetSubmesh(s).ToMesh();
                    if (s == 0) {
                        meshFilter.sharedMesh = submesh;
                        renderer.sharedMaterials = new Material[] { materials[s] };
                    }
                    else {
                        var child = new GameObject(string.IsNullOrEmpty(materials[s].name) ? "Unamed Material" : materials[s].name);
                        child.transform.SetParent(gameObject.transform);
                        child.transform.localPosition = Vector3.zero;
                        child.transform.localRotation = Quaternion.identity;
                        child.transform.localScale = Vector3.one;
                        child.GetOrAddComponent<MeshFilter>().sharedMesh = submesh;
                        child.GetOrAddComponent<MeshRenderer>().sharedMaterials = new Material[] { materials[s] };
                        newGameObjects.Add(child);
                    }
                }
            } else {
                for (int s = 0; s < mesh.subMeshCount; s++) {
                    Mesh submesh = mesh.GetSubmesh(s).ToMesh();
                    var child = new GameObject(string.IsNullOrEmpty(materials[s].name) ? "Unamed Material" : materials[s].name);
                    child.transform.SetParent(gameObject.transform);
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localRotation = Quaternion.identity;
                    child.transform.localScale = Vector3.one;
                    child.GetOrAddComponent<MeshFilter>().sharedMesh = submesh;
                    child.GetOrAddComponent<MeshRenderer>().sharedMaterials = new Material[] { materials[s] };
                    newGameObjects.Add(child);
                }
                GameObject.DestroyImmediate(renderer);
                GameObject.DestroyImmediate(meshFilter);
            }

            return newGameObjects;
        }

        /// <summary>
        /// Returns the Component of the given type. It will add a new one if it doesn't exists.
        /// </summary>
        /// <typeparam name="T">Component Type</typeparam>
        /// <param name="gameObject">GameObject</param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (!component) {
#if UNITY_EDITOR
                try {
                    component = UnityEditor.Undo.AddComponent<T>(gameObject);
                } catch { }
#else
                component = gameObject.AddComponent<T>();
#endif
            }
            return component;
        }

        /// <summary>
        /// Returns all children (recursive) of given GameObjects
        /// </summary>
        /// <param name="gameObjects">Input GameObjects</param>
        /// <param name="recursive">If true, it will gather children recursively</param>
        /// <param name="includeParent">If true, input GameObjects will be present in the output</param>
        /// <returns></returns>
        public static IList<GameObject> GetChildren(this IList<GameObject> gameObjects, bool recursive, bool includeParent) {
            HashSet<GameObject> output = new HashSet<GameObject>();
            for (int i = 0; i < gameObjects.Count; i++) {
                foreach (GameObject gameOject in gameObjects[i].GetChildren(recursive, false)) {
                    output.Add(gameOject);
                }
                if (includeParent)
                    output.Add(gameObjects[i]);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Returns all children (recursive) of given GameObjects
        /// </summary>
        /// <param name="gameObjects">Input GameObject</param>
        /// <param name="recursive">If true, it will gather children recursively</param>
        /// <param name="includeParent">If true, input GameObject will be present in the output</param>
        /// <returns></returns>
        public static List<GameObject> GetChildren(this GameObject gameObject, bool recursive, bool includeParent) {
            List<GameObject> output = new List<GameObject>();
            if (includeParent)
                output.Add(gameObject);
            if (gameObject.transform.childCount == 0)
                return output;
            Stack<GameObject> stack = new Stack<GameObject>();
            stack.Push(gameObject);
            while (stack.Count != 0) {
                GameObject current = stack.Pop();
                foreach (Transform child in current.transform) {
                    stack.Push(child.gameObject);
                    output.Add(child.gameObject);
                }
                if (!recursive)
                    break;
            }
            return output;
        }

        /// <summary>
        /// Returns all GameObjects in given layer
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameObjectsInLayer(int layer) {
            var gameObjects = new List<GameObject>();
            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>()) {
                if (go.layer == layer) {
                    gameObjects.Add(go.gameObject);
                }
            }
            return gameObjects;
        }

        /// <summary>
        /// Check if the localPosition is Vector3(0,0,0), localRotation is Identity and localScale is Vector(1,1,1)
        /// </summary>
        /// <param name="Transform"></param>
        /// <returns></returns>
        public static bool LocalIsIdentity(this Transform transform) {
            return transform.localPosition == Vector3.zero && transform.localRotation == Quaternion.identity && transform.localScale == Vector3.one;
        }

        /// <summary>
        /// Returns the highest ancestor (hierarchically) of a list of GameObjects 
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <returns></returns>
        public static GameObject GetHighestAncestor(IList<GameObject> gameObjects) {
            var selectedGameObjects = new HashSet<GameObject>(gameObjects);
            GameObject highestSelectedAncestor = null;
            int highestSelectedLevel = int.MaxValue;
            for (int i = 0; i < gameObjects.Count; i++) {
                Transform current = gameObjects[i].transform;
                int backPropagations = 0;
                int highestSelectedBackPropagationLevel = 0;
                GameObject localHighestSelectedAncestor = null;
                while (current) {
                    if (selectedGameObjects.Contains(current.gameObject)) {
                        localHighestSelectedAncestor = current.gameObject;
                        highestSelectedBackPropagationLevel = backPropagations;
                    }
                    current = current.parent;
                    backPropagations++;
                }
                int localHighestSelectedLevel = backPropagations - highestSelectedBackPropagationLevel;
                if (localHighestSelectedLevel < highestSelectedLevel) {
                    highestSelectedAncestor = localHighestSelectedAncestor;
                }
            }
            return highestSelectedAncestor;
        }

#if UNITY_EDITOR
        private static bool ShowUnpackDialog()
        {
            return UnityEditor.EditorUtility.DisplayDialog("Invalid prefab operation", "Pixyz has detected that at least one of the GameObjects is linked to a prefab. However, the running action will break the prefab instance.\n\n Would you like to unpack the prefab and continue ?", "Yes, unpack and continue", "No, abort");
        }
#endif

        /// <summary>
        /// Change Tranform's parent, but ensures it is properly unpack if transform is part of a prefab (if editor)
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        public static void SetParentSafe(this Transform transform, Transform parent, bool worldPositionStays = true)
        {
            if (!transform)
                throw new NullReferenceException();
            if (transform == parent)
                return;
#if UNITY_EDITOR
            var transformPrefabStatus = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(transform);
            if (transformPrefabStatus == UnityEditor.PrefabInstanceStatus.Connected || transformPrefabStatus == UnityEditor.PrefabInstanceStatus.MissingAsset) {
                if (ShowUnpackDialog()) {
                    UnityEditor.PrefabUtility.UnpackPrefabInstance(UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(transform.gameObject), UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
                } else {
                    throw new Tools.PrefabModificationException();
                }
            }
            var parentPrefabStatus = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(transform);
            if (parentPrefabStatus == UnityEditor.PrefabInstanceStatus.Connected || parentPrefabStatus == UnityEditor.PrefabInstanceStatus.MissingAsset) {
                if (ShowUnpackDialog()) {
                    UnityEditor.PrefabUtility.UnpackPrefabInstance(UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(parent.gameObject), UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
                } else {
                    throw new Tools.PrefabModificationException();
                }
            }
#endif
            transform.SetParent(parent, worldPositionStays);
        }

        /// <summary>
        /// Destroys GameObject, but ensure that it is properly unpacked if gameobject is part of a prefab (if editor)
        /// </summary>
        /// <param name="gameObject"></param>
        public static void DestroyImmediateSafe(this GameObject gameObject)
        {
            if (!gameObject)
                return;
#if UNITY_EDITOR
            var prefabStatus = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (prefabStatus == UnityEditor.PrefabInstanceStatus.Connected || prefabStatus == UnityEditor.PrefabInstanceStatus.MissingAsset) {
                if (ShowUnpackDialog()) {
                    UnityEditor.PrefabUtility.UnpackPrefabInstance(UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(gameObject), UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
                } else {
                    throw new Tools.PrefabModificationException();
                }
            }
#endif
            GameObject.DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Context adapted delegate for instantiating Unity Objects.
        /// In runtime, it will create a clone.
        /// In editor, it will instantiate the prefab.
        /// </summary>
        public static GameObject Instantiate(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject) == UnityEditor.PrefabInstanceStatus.NotAPrefab) {
                return UnityEngine.Object.Instantiate(gameObject);
            } else {
                return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(gameObject);
            }
#else
            return UnityEngine.Object.Instantiate(gameObject);
#endif
        }

        public static Shader GetDefaultShader()
        {
            Shader shader = null;
#if UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset) {
                shader = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.defaultShader;
            }
#else
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset) {
                shader = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetDefaultShader();
            }
#endif
            if (shader == null)
                shader = Shader.Find("Standard");
            return shader;
        }
    }
}