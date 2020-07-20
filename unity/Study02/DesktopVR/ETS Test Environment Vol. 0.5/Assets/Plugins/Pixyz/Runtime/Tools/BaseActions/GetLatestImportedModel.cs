using Pixyz.Import;
using System;
using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    /// <summary>
    /// Get the latest GameObject imported with Pixyz.\nIt will fail if there isn't at least one model imported with Pixyz in the current scene.\nUse [Include Whole Hierarchy] to also include all of its children as well (at any level).
    /// </summary>
    public class GetLatestImportedModel : ActionOut<IList<GameObject>> {

        [UserParameter]
        public bool includeWholeHierarchy = true;

        public override int id => 246456413;
        public override string menuPathRuleEngine => "Get/Latest Imported Model";
        public override string menuPathToolbox => null;
        public override string tooltip => "Get the latest GameObject imported with Pixyz.\nIt will fail if there isn't at least one model imported with Pixyz in the current scene.\nUse [Include Whole Hierarchy] to also include all of its children as well (at any level).";

        public override IList<GameObject> run() {
            if (!Configuration.CheckLicense())
                throw new NoValidLicenseException();

            IList<GameObject> gameObjects;

            if (includeWholeHierarchy) {
                gameObjects = Importer.LatestModelImportedObject.gameObject.GetChildren(true, true);
            } else {
                gameObjects = new GameObject[] { Importer.LatestModelImportedObject.gameObject };
            }

#if UNITY_EDITOR
            foreach (GameObject go in gameObjects) {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(go, "RuleEngine Entry Point");
            }
#endif

            return gameObjects;
        }

        public override IList<string> getWarnings()
        {
            if (Importer.LatestModelImportedObject == null) {
                return new string[] { "There are currently no latest model imported in scene" };
            }
            return new string[0];
        }
    }
}