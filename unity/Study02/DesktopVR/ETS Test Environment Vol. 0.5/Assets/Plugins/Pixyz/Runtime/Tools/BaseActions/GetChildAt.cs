using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin {

    public class GetChildAt : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool keepParent = false;

        [UserParameter]
        public int index = 0;

        public override int id { get { return 11954093; } }
        public override string menuPathRuleEngine => "Get/Child at"; 
        public override string menuPathToolbox => null; 
        public override string tooltip => "Get the child at the position specified by the 'Index' (if it exists). Use 'Keep Parent' to keep parents from the input in the output."; 

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            List<GameObject> gameObjects = new List<GameObject>();
            foreach (GameObject gameObject in input) {
                int i = 0;
                foreach (Transform child in gameObject.transform) {
                    if (index == i) {
                        gameObjects.Add(child.gameObject);
                        break;
                    }
                    i++;
                }
                if (keepParent) {
                    gameObjects.Add(gameObject);
                }
            }

#if UNITY_EDITOR
            foreach (GameObject go in gameObjects) {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(go, "RuleEngine Entry Point");
            }
#endif

            return gameObjects;
        }
    }
}