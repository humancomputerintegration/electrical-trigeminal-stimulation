using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Pixyz.Plugin4Unity;
using Pixyz.Editor;

namespace Pixyz.Import.Editor
{
    public class PostprocessVRED : SubProcess {

        [InitializeOnLoadMethod]
        private static void RegisterPostProcess() {
            var postprocess = new PostprocessVRED();
            postprocess.initialize();
            Importer.AddOrSetPostprocess(".vpb", postprocess);
        }

        public override int id { get { return 870686692; } }
        public override string menuPathRuleEngine { get { return null; } }
        public override string menuPathToolbox { get { return null; } }

        public override string name => "VRED Import Settings";

        public override void onBeforeDraw(ImportSettings importSettings)
        {

        }

        public override void run(Importer importer) {

            PreprocessVRED preprocess = importer.preprocess as PreprocessVRED;
            if (preprocess == null) {
                throw new System.Exception("PostprocessVRED requires PreprocessVRED to work.");
            }

            importer.importedModel.transform.Rotate(new Vector3(-90f, 0f));
            Metadata metadata = importer.importedModel.GetComponent<Metadata>();

            // Import and merge animations
            if (preprocess.importAnimations) {
                try {
                    string animationPath = metadata?.getProperty("PXZ_ANIM_FBX_FILE");
                    mergeAnimations(importer.importedModel, animationPath);

                    // Compensate unity fbx import
                    importer.importedModel.transform.rotation = Quaternion.identity;
                    importer.importedModel.transform.localScale = Vector3.one;
                } catch (System.Exception ex) {
                    Debug.Log("Failed to import animations: " + ex);
                }
            }

            if (preprocess.importVariants) {
                // Initialize variants manager
                try {
                    string varPath = metadata?.getProperty("PXZ_MATERIAL_CSV_FILE");
                    importer.importedModel.gameObject.AddComponent<VariantsManager>().initVariantsManager(varPath);
                } catch (System.Exception ex) {
                    Debug.Log("Failed initializing variants manager: " + ex);
                }

                try {
                    string vsPath = metadata?.getProperty("PXZ_VARSETS_XML_FILE");
                    importer.importedModel.gameObject.AddComponent<VariantSets>().initVariantSets(vsPath);
                } catch (System.Exception ex)
                {
                    Debug.Log("Failed initializing variant sets: " + ex);
                }
            }

            metadata?.removeProperty("PXZ_ANIM_FBX_FILE");
            metadata?.removeProperty("PXZ_MATERIAL_CSV_FILE");
            metadata?.removeProperty("PXZ_VARSETS_XML_FILE");
        }

        /// <summary>
        /// Merge animation file (fbx) with imported model.
        /// Keep imported model root (with ImportStamp component) and merge all animated nodes into its tree
        /// </summary>
        /// <param name="importedModel"></param>
        /// <param name="animationPath"></param>
        private void mergeAnimations(ImportStamp importedModel, string animationPath) {

            // Load animation file at animationPath
            Transform animationRoot = loadAnimation(animationPath, Path.GetFileNameWithoutExtension(importedModel.fullPath));
            var animChildrenNames = new List<string>();
            foreach (Transform child in animationRoot) { animChildrenNames.Add(child.name); }

            Transform pxzRoot = getAnimatedRoot(importedModel.transform, animChildrenNames);
            if (animationRoot == null || pxzRoot == null) {
                Debug.Log("Unsuccessfull animation import");
                return;
            }

            // Merge trees in animation root
            try { merge(animationRoot, pxzRoot.transform); }
            catch { Debug.Log("Unsuccessfull animation merge; Product structure might be affected"); return; }

            animationRoot.name = pxzRoot.name;
            animationRoot.SetParent(pxzRoot.transform.parent, false);
            GameObject.DestroyImmediate(pxzRoot.gameObject);
        }

        private Transform getAnimatedRoot(Transform pxzRoot, List<string> animChildrenNames)
        {
            if (pxzRoot.childCount == 0) return null;
            var rootChildrenNames = new List<string>();
            foreach (Transform child in pxzRoot) { rootChildrenNames.Add(child.name); }

            if (rootChildrenNames.Count >= animChildrenNames.Count) {
                bool same = true;
                foreach (string animName in animChildrenNames) {
                    if (!rootChildrenNames.Contains(animName)) {
                        same = false;
                        break;
                    }
                }
                if (same) return pxzRoot;
            }
            foreach (Transform child in pxzRoot) {
                var candidate = getAnimatedRoot(child, animChildrenNames);
                if (candidate != null) return candidate;
            }
            return null;
        }

        /// <summary>
        /// Load animation file at animationPath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        private Transform loadAnimation(string filePath, string newName)
        {

            // Move file to asset database
            if (!Directory.Exists(Application.dataPath + "/" + Preferences.PrefabFolder)) { Directory.CreateDirectory(Application.dataPath + "/" + Preferences.PrefabFolder); }
            File.Copy(filePath, Application.dataPath + "/" + Preferences.PrefabFolder + "/" + newName + ".anim.fbx", true);

            string relativeFilePath = "Assets/" + Preferences.PrefabFolder + "/" + newName + ".anim.fbx";

            AssetDatabase.Refresh();
            var animationModel = AssetDatabase.LoadAssetAtPath(relativeFilePath, typeof(Object)) as GameObject;
            if (animationModel == null)
            {
                Debug.LogError("Fail to load " + relativeFilePath);
                if(!System.IO.File.Exists(relativeFilePath))
                    Debug.Log("This file does not exists : "+ relativeFilePath);
                return null;
            }
            animationModel = Object.Instantiate(animationModel);
            animationModel.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            return animationModel.transform;
        } 

        /// <summary>
        /// Merge trees recursively
        /// Check if pxzRoot has metadata, if so copy them to animation root
        /// Parse both children of animationRoot and pxzRoot
        /// If child in common, recursive call on them
        /// --> The function will be called on all common nodes
        /// Else
        /// --> Set parent as animationRoot
        /// </summary>
        /// <param name="animationRoot"></param>
        /// <param name="pxzRoot"></param>
        private void merge(Transform animationRoot, Transform pxzRoot)
        {
            if (pxzRoot.GetComponent<Metadata>())
            {
                Metadata mtd = animationRoot.gameObject.AddComponent<Metadata>();
                var nprop = new NativeInterface.Properties();
                nprop.values = new NativeInterface.StringList(0);
                nprop.names = new NativeInterface.StringList(0);
                mtd.setProperties(nprop);
                foreach (KeyValuePair<string, string> property in pxzRoot.GetComponent<Metadata>().getProperties()){
                    animationRoot.GetComponent<Metadata>().addOrSetProperty(property.Key, property.Value);
                }
            }

            List<string> animationChildrenNames = new List<string>();
            foreach (Transform pxzChild in pxzRoot)
            {
                foreach (Transform animationChild in animationRoot)
                {
                    animationChildrenNames.Add(animationChild.name);
                    if (animationChild.name.Contains(pxzChild.name))
                    {
                        merge(animationChild, pxzChild);
                    }
                }
            }
            
            // First loop: put the child to export in an array
            // Second loop: export the child
            // Why two loops? If directly exported, the first loop will loose its references
            List<Transform> childrenToExport = new List<Transform>();
            foreach (Transform pxzChild in pxzRoot)
            {
                if (!animationChildrenNames.Contains(pxzChild.name))
                {
                    childrenToExport.Add(pxzChild);
                }
            }

            foreach (Transform pxzChild in childrenToExport)
            {
                pxzChild.parent = animationRoot;
            }
        }
    }
}