using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class AddChild : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public GameObject newChild;

        [UserParameter]
        public Placement placement = Placement.Pivot;

        //[UserParameter]
        //public bool ensureUnique = true;

        public enum Placement { Pivot, Center }

        public override int id => 561784150;
        public override string menuPathRuleEngine => "Add/Child";
        public override string menuPathToolbox => null;
        public override string tooltip => "Add child to input GameObjects.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            List<GameObject> gameObjects = new List<GameObject>(input);
            for (int i = 0; i < input.Count; i++) {

                if (placement == Placement.Pivot) {
                    GameObject newChildInstance = GameObject.Instantiate<GameObject>(newChild);
                    newChildInstance.transform.SetParentSafe(gameObjects[i].transform, false);
                    newChildInstance.transform.localEulerAngles = Vector3.zero;
                    newChildInstance.transform.localScale = newChild.transform.localScale;
                    newChildInstance.transform.localPosition = Vector3.zero;
                } else {
                    Bounds bounds = new Bounds(gameObjects[i].transform.position, Vector3.zero);
                    Renderer[] renderers = gameObjects[i].GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers) {
                        bounds.Encapsulate(renderer.bounds);
                    }
                    GameObject newChildInstance = GameObject.Instantiate<GameObject>(newChild);
                    newChildInstance.transform.SetParentSafe(gameObjects[i].transform, false);
                    newChildInstance.transform.localEulerAngles = Vector3.zero;
                    newChildInstance.transform.localScale = newChild.transform.localScale;
                    newChildInstance.transform.position = bounds.center;
                }
            }
            return gameObjects.ToArray();
        }

        public override IList<string> getErrors() {
            if (newChild == null)
                return new string[] { "A GameObject reference is required !" };
            return null;
        }
    }

}