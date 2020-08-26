using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Pixyz.Utils;
using System.IO;

namespace Pixyz.Import {

    /// <summary>
    /// Class handling transform and material switches imported from Autodesk VRED
    /// </summary>
    [Serializable]
    public sealed class VariantsManager : MonoBehaviour {

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] List<MaterialSwitch> materialSwitchList = new List<MaterialSwitch>();
        public List<MaterialSwitch> MaterialSwitchList { get { return materialSwitchList; } }

        [SerializeField] List<TransformSwitch> transformSwitchList = new List<TransformSwitch>();
        public List<TransformSwitch> TransformSwitchList { get { return transformSwitchList; } }

        private const string materialSwitchTag = "_PiXYZ_MATERIAL_SWITCH_TAG";
        private const string transformSwitchTag = "_PiXYZ_TRANSFORM_SWITCH_TAG";
        private const string pxzPool = "_PiXYZ_MATERIAL_POOL";

        public abstract class Switch {
            public abstract bool selectVariant(object variant);
        }

        [Serializable]
        public abstract class Switch<T>: Switch {
            
            [HideInInspector]
            public string name;

            [HideInInspector]
            protected T state;

            public List<T> variants;

            protected Switch(string n) { name = n; }

            /// <summary>
            /// Returns the currently selected variant
            /// </summary>
            /// <returns></returns>
            public T getState() { return state; }

            /// <summary>
            /// Returns the variants handled by the switch
            /// </summary>
            /// <returns></returns>
            public List<T> getVariants() { return variants; }
        }

        /// <summary>
        /// Class handling transform variants
        /// </summary>
        [Serializable]
        public class TransformSwitch : Switch<TransformVariant> {

            public TransformSwitch(GameObject switchObject) : base(switchObject.name)
            {
                List<GameObject> variantsObject = switchObject.gameObject.GetChildren(false, false);
                variants = new List<TransformVariant>();
                foreach (GameObject variant in variantsObject) {
                    TransformVariant transformVariant = variant.AddComponent<TransformVariant>();
                    transformVariant.SetSwitch(this);
                    transformVariant.SetKey(name);
                    variants.Add(transformVariant);
                }
                if (variants.Count != 0) selectVariant(variants[0]);
                else state = null;
            }

            /// <summary>
            /// Selects the transform variant passed as parameter
            /// </summary>
            /// <param name="variant"></param>
            /// <returns>True if success</returns>
            public override bool selectVariant(object variant)
            {
                if (!(variant is TransformVariant)) { Debug.Log("Wrong type"); return false; }
                foreach (TransformVariant var in variants) {

                    if (var == variant as TransformVariant) {
                        var.gameObject.SetActive(true);
                        // If a parent variant is disabled, var.setActive(true) wont trigger the OnEnable function
                        // --> Check if object is active in hiearchy --> if not, force ObjectEnabled call
                        if (!var.gameObject.activeInHierarchy) {
                            variantEnabled(var);
                        }
                        state = var;
                        return true;
                    }
                }
                Debug.Log("The variant is not part of " + name);
                return false;
            }

            public void variantEnabled(TransformVariant enabled_variant)
            {
                foreach (TransformVariant variant in variants) {
                    if (variant.gameObject != null && variant.gameObject.activeSelf && variant != enabled_variant) {
                        variant.gameObject.SetActive(false);
                    }
                }
            }

            public void objectDisabled(TransformVariant disabled_variant)
            {
                int index = variants.IndexOf(disabled_variant);
                variants[(index + 1) % variants.Count].gameObject.SetActive(true);
            }

        }

        /// <summary>
        /// Class handling material variants
        /// </summary>
        [Serializable]
        public class MaterialSwitch : Switch<Material> {
            [HideInInspector] [SerializeField]
            private List<GameObject> parts = new List<GameObject>();

            public MaterialSwitch(GameObject root, string switchName, string materialVariantsFile) : base(switchName)
            {
                List<Material> resourcesMaterials = loadAllMaterialsFromResources();
                List<Material> poolMaterials = loadAllMaterialsFromPool();
                resourcesMaterials.AddRange(poolMaterials);

                List<string> materialsVariantsNames = getMaterialVariantsNames(materialVariantsFile, switchName);
                variants = getMaterialsWithNames(materialsVariantsNames, resourcesMaterials);
                parts = getObjectsWithPropertyAndValue(root, materialSwitchTag, name);

                if (variants.Count != 0) selectVariant(variants[0]);
                else state = null;
            }

            /// <summary>
            /// Selects the material variant passed as parameter
            /// </summary>
            /// <param name="variant"></param>
            /// <returns>True if success</returns>
            public override bool selectVariant(object variant)
            {
                if (!(variant is Material)) { Debug.Log("Wrong type"); return false; }
                Material target = null;
                foreach (Material mat in variants) {
                    if (mat == variant as Material) {
                        target = mat;
                        break;
                    }
                }
                if (target == null) { Debug.Log("The variant is not part of " + name); return false; }
                foreach (GameObject part in parts) {
                    if (part.GetComponent<Renderer>() != null) {
                        part.GetComponent<Renderer>().material = target;
                    }
                }
                state = target;
                return true;
            }
        }


        public void initVariantsManager(string materialVariantsFile)
        {
            List<Metadata> metadataObjects = new List<Metadata>(GetComponentsInChildren<Metadata>());
            List<GameObject> materialSwitchObjects = getObjectsWithPropertyAndValue(this.gameObject, materialSwitchTag);
            List<GameObject> transformSwitchObjects = getObjectsWithPropertyAndValue(this.gameObject, transformSwitchTag);

            initMaterialSwitchList(materialSwitchObjects, materialVariantsFile);
            initTransformSwitchList(transformSwitchObjects);
        }

        private void initMaterialSwitchList(List<GameObject> switchObjects, string materialVariantsFile)
        {
            List<string> switches = new List<string>();
            foreach (GameObject part in switchObjects) {
                Metadata mtd = part.GetComponent<Metadata>();
                if (mtd == null) continue;
                string switchName = mtd.getProperty(materialSwitchTag);
                if (!switches.Contains(switchName)) {
                    materialSwitchList.Add(new MaterialSwitch(this.gameObject, switchName, materialVariantsFile));
                    switches.Add(switchName);
                }

            }
        }

        private void initTransformSwitchList(List<GameObject> switchObjects)
        {
            List<string> switches = new List<string>();
            foreach (GameObject switchObject in switchObjects) {
                string switchName = switchObject.name;
                while (switches.Contains(name)) { switchName += "_"; }
                transformSwitchList.Add(new TransformSwitch(switchObject));
                switches.Add(switchName);
            }
        }

        // Filter a list of Metadata with property and value
        // If value is not defined, will only filter on property
        static private List<GameObject> getObjectsWithPropertyAndValue(GameObject root, string property, string value = null)
        {
            List<Metadata> metadatas = new List<Metadata>(root.GetComponentsInChildren<Metadata>());
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (Metadata meta in metadatas) {
                Dictionary<string, string> dict = meta.getProperties();
                if (dict.ContainsKey(property) && value != null && dict[property].Equals(value) && !gameObjects.Contains(meta.gameObject)) {
                    gameObjects.Add(meta.gameObject);
                } else if (dict.ContainsKey(property) && value == null && !gameObjects.Contains(meta.gameObject)) {
                    gameObjects.Add(meta.gameObject);
                }
            }
            return gameObjects;
        }

        // Gets materials with names
        // names : list of wanted materials
        // materials : list of materials to search
        static List<Material> getMaterialsWithNames(List<string> names, List<Material> materials)
        {
            List<Material> correspondance = new List<Material>();
            foreach (string name in names) {
                Material material = materials.Find(i => i.name.Replace(" (Instance)", "") == name);
                if (material != null) correspondance.Add(material);
            }
            return correspondance;
        }

        // Returns the material variants names corresponding to the swith material name switchKey
        // string materialVariantsFile : csv file to read variants from
        // string switchKey : switch material name
        static List<string> getMaterialVariantsNames(string materialVariantsFile, string switchKey)
        {
            List<string> properties = new List<string>();
            StreamReader reader = new StreamReader(File.OpenRead(materialVariantsFile));
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                if (line.StartsWith(switchKey)) {
                    string[] values = line.Split(';');
                    if (values.Length > 1) {
                        for (int i = 1; i < values.Length; i++) { properties.Add(values[i]); }
                    }
                }
            }
            return properties;
        }

        // Parse pxzPool and load all materials from it
        static List<Material> loadAllMaterialsFromPool()
        {
            List<Material> materials = new List<Material>();
            Scene scene = SceneManager.GetActiveScene();
            var pxzModel = Importer.LatestModelImportedObject;
            GameObject pool = findPool(pxzModel.gameObject);
            if (pool == null) return materials;
            foreach (GameObject materialObject in pool.GetChildren(false, false)) {
                if (materialObject.GetComponent<Renderer>() != null) {
                    materials.Add(materialObject.GetComponent<Renderer>().sharedMaterial);
                }
            }
            return materials;
        }

        // Parse Resources folder and returns all its materials objects
        static List<Material> loadAllMaterialsFromResources()
        {
            UnityEngine.Object[] objects = Resources.LoadAll("", typeof(Material));
            List<Material> materials = new List<Material>();
            foreach (Material mat in objects) {
                materials.Add(mat);
            }
            return materials;
        }

        // Finds the pxzPool recursively
        // GameObject node : root of the imported gameObject
        static GameObject findPool(GameObject node)
        {
            GameObject pool = null;
            if (node.name == pxzPool) return node;
            var children = node.GetChildren(false, false);
            foreach (var child in children) {
                pool = findPool(child);
                if (pool != null) return pool;
            }
            return pool;
        }
    }
}
