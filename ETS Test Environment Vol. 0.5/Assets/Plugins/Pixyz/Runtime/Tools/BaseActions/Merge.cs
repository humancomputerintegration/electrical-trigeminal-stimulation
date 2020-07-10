using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;
using System.Text.RegularExpressions;

namespace Pixyz.Tools.Builtin
{

    public class Merge : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool keepParent = false;

        public override int id => 511763496;
        public override int order => 2;
        public override string menuPathRuleEngine => "Scene/Merge";
        public override string menuPathToolbox => "Merge";
        public override string tooltip => "Merges GameObjects together. 'All Together' will merge all input objects into a single object. 'With Children' will merge each with all of its own children (recursively). By merging, you will lose additional components such as Metadatas.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            Regex regex = new Regex("_LOD[1-9]$");

            if (keepParent) {
                HashSet<GameObject> selectedGameObjects = new HashSet<GameObject>(input);
                HashSet<GameObject> highestSelectedAncestors = new HashSet<GameObject>();
                for (int i = 0; i < input.Count; i++) {
                    Transform current = input[i].transform;
                    GameObject highestSelectedAncestor = null;
                    while (current) {
                        if (selectedGameObjects.Contains(current.gameObject)) {
                            highestSelectedAncestor = current.gameObject;
                        }
                        current = current.parent;
                    }
                    highestSelectedAncestors.Add(highestSelectedAncestor);
                }

                foreach (GameObject gameObject in highestSelectedAncestors) {
                    gameObject.MergeChildren();
                }

                return highestSelectedAncestors.ToArray();

            } else {
                
                GameObject highestSelectedAncestor = SceneExtensions.GetHighestAncestor(input);

                MergingContainer meshTransfer = new MergingContainer();

                for (int i = 0; i < input.Count; i++) {

                    if (!input[i])
                        continue;

                    if (!highestSelectedAncestor)
                        continue;

                    if (!regex.IsMatch(input[i].name)) { // Don't merge LODs lower than 0
                        meshTransfer.addGameObject(input[i], highestSelectedAncestor.transform);
                    }

                    if (input[i] == highestSelectedAncestor)
                        continue;

                    foreach (Transform child in input[i].transform.GetEnumerator().ToEnumerable<Transform>().ToArray()) {
                        if (input[i].transform.parent) {
                            child.SetParentSafe(input[i].transform.parent, true);
                        } else {
                            child.SetParentSafe(null, true);
                        }
                    }
                }

                if (meshTransfer.vertexCount > 0) {
                    highestSelectedAncestor.GetOrAddComponent<MeshFilter>().sharedMesh = meshTransfer.getMesh();
                    highestSelectedAncestor.GetOrAddComponent<MeshRenderer>().sharedMaterials = meshTransfer.sharedMaterials;
                }

                for (int i = 0; i < input.Count; i++) {
                    if (input[i] != highestSelectedAncestor) {
                        SceneExtensions.DestroyImmediateSafe(input[i]);
                    }
                }

                return new GameObject[] { highestSelectedAncestor };
            }
        }
    }
}