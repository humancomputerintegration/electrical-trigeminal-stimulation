using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Tools.Builtin {

    public class GetAllGameObjects : ActionOut<IList<GameObject>> {

        public override int id => 887274667;
        public override string menuPathRuleEngine => "Get/All GameObjects";
        public override string menuPathToolbox => null;
        public override string tooltip => "Get all GameObjects in the current Scene, at any level.";

        public override IList<GameObject> run() {

            var gameObjects = GameObject.FindObjectsOfType<GameObject>();

#if UNITY_EDITOR
            foreach (GameObject go in gameObjects) {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(go, "RuleEngine Entry Point");
            }
#endif

            return gameObjects;
        }
    }
}