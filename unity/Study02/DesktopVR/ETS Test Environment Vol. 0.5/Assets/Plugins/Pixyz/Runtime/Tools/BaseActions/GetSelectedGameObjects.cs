using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Tools.Builtin
{
    public class GetSelectedGameObjects : ActionOut<IList<GameObject>> {

        public override int id => 64811152;
        public override string menuPathRuleEngine => "Get/Selected GameObjects";
        public override string menuPathToolbox => null;
        public override string tooltip => "Get GameObjects selected in the Unity Editor";

        public override IList<GameObject> run() {

            IList<GameObject> gameObjects;

#if UNITY_EDITOR
            gameObjects = UnityEditor.Selection.gameObjects;
#else
            gameObjects = new GameObject[0];
#endif

#if UNITY_EDITOR
            foreach (GameObject go in gameObjects) {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(go, "RuleEngine Entry Point");
            }
#endif

            return gameObjects;
        }
    }
}