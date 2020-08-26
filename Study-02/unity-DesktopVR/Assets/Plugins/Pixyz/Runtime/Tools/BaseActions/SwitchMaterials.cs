using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class SwitchMaterials : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public CopyReplace mode = CopyReplace.ReplaceReferences;

        [UserParameter]
        public MaterialRemplacement[] materialRemplacements;

#if UNITY_EDITOR
        [HelperMethod]
        public void addSelectedMaterials() {
            var materials = UnityEditor.Selection.gameObjects.GetChildren(true, true).GetMaterials();
            HashSet<MaterialRemplacement> uniques = new HashSet<MaterialRemplacement>(materialRemplacements);
            for (int i = 0; i < materials.Count(); i++) {
                uniques.Add(new MaterialRemplacement { materialName = materials[i].name });
            }
            materialRemplacements = uniques.ToArray();
        }

        [HelperMethod]
        public void fillWithAllMaterialsInScene() {
            var materials = GameObject.FindObjectsOfType<GameObject>().GetMaterials();
            HashSet<MaterialRemplacement> uniques = new HashSet<MaterialRemplacement>(materialRemplacements);
            for (int i = 0; i < materials.Count(); i++) {
                uniques.Add(new MaterialRemplacement { materialName = materials[i].name, unityMaterial = materials[i] });
            }
            materialRemplacements = uniques.ToArray();
        }

        [HelperMethod]
        public void fillFromCSV() {
            List<MaterialRemplacement> materialRemplacements = new List<MaterialRemplacement>();
            string csvFile = (FilePath)Pixyz.Editor.ReferencableEditorUtils.SelectFile(new string[] { "CSV File", "csv" });
            if (string.IsNullOrEmpty(csvFile)) {
                return;
            }
            var lines = File.ReadAllLines(csvFile);
            for (int i = 0; i < lines.Length; i++) {
                string[] splitLine = lines[i].Split(new char[] { ',', ';' });
                materialRemplacements.Add(new MaterialRemplacement { materialName = splitLine[0].Trim(), unityMaterial = (splitLine.Length < 2) ? null : Resources.Load<Material>(splitLine[1].Trim()) });
            }
            this.materialRemplacements = materialRemplacements.ToArray();
        }

        [HelperMethod]
        public void resetMaterials() {
            materialRemplacements = new MaterialRemplacement[0];
        }
#endif

        public override int id => 315214542;
        public override string menuPathRuleEngine => "Modify/Switch Materials";
        public override string menuPathToolbox => null;
        public override string tooltip => "Switch Materials from their Names with Materials in the current project. 'Replace References' mode will replace the materials, and 'Copy Properties' will copy the properties (shader, color, intensities, ...) from the project Material to the Material applied. When copying properties, names of original Materials is kept.";

        public struct MaterialRemplacement {
            public string materialName;
            public Material unityMaterial;
        }

        public enum CopyReplace {
            CopyProperties, ReplaceReferences
        }

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            for (int i = 0; i < input.Count; i++) {
                MeshRenderer meshRenderer = input[i].GetComponent<MeshRenderer>();
                if (!meshRenderer)
                    continue;
                Material[] meshMaterials = meshRenderer.sharedMaterials;
                bool modified = false;
                for (int m = 0; m < meshMaterials.Length; m++) {
                    for (int j = 0; j < materialRemplacements.Length; j++) {
                        if (meshMaterials[m].name == materialRemplacements[j].materialName && materialRemplacements[j].unityMaterial != null) {
                            // Switch material reference
                            if (mode == CopyReplace.ReplaceReferences) {
                                meshMaterials[m] = materialRemplacements[j].unityMaterial;
                                modified = true;
                            } else {
                                if (meshMaterials[m] != materialRemplacements[j].unityMaterial) {
                                    meshMaterials[m].shader = materialRemplacements[j].unityMaterial.shader;
                                    meshMaterials[m].CopyPropertiesFromMaterial(materialRemplacements[j].unityMaterial);
                                }
                            }
                            break;
                        }
                    }
                }
                if (modified) {
                    // Apply modifications for this GameObject
                    meshRenderer.sharedMaterials = meshMaterials;
                }
            }
            return input;
        }

        public override IList<string> getWarnings() {
            if (materialRemplacements == null)
                return new string[] { "Add at least one material remplacement" };
            List<string> errors = new List<string>();
            for (int k = 0; k < materialRemplacements.Length; k++) {
                if (materialRemplacements[k].unityMaterial == null)
                    errors.Add($"Material Remplacement n°{k + 1} is required !");
            }
            return errors.ToArray();
        }
    }
}