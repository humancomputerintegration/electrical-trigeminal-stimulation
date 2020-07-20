using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin {

    public class GetGameObject : ActionOut<IList<GameObject>> {

        [UserParameter]
        public GameObject gameobject;

        [UserParameter]
        public bool includeWholeHierarchy = true;

        public override int id => 941545872;
        public override string menuPathRuleEngine => "Get/GameObject";
        public override string menuPathToolbox => null;
        public override string tooltip => "Starts with the given scene GameObject. Use 'Include Whole Hierarchy' to also include all of its children.";

        public override IList<GameObject> run() {
            if (!Configuration.CheckLicense())
                throw new NoValidLicenseException();

            IList<GameObject> gameObjects;

            if (includeWholeHierarchy) {
                gameObjects = gameobject.GetChildren(true, true);
            } else {
                gameObjects = new GameObject[] { gameobject };
            }

    #if UNITY_EDITOR
            foreach (GameObject go in gameObjects) {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(go, "RuleEngine Entry Point");
            }
    #endif

            return gameObjects;
        }

        public override IList<string> getErrors() {
            if (gameobject == null) {
                return new string[] { "You need to specify a GameObject." };
            }
            return null;
        }
    }
}