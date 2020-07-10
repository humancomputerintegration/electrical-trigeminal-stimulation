using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class Delete : ActionIn<IList<GameObject>> {

        [UserParameter]
        public bool alsoDeleteChildren = true;

        public override int id => 16549960;
        public override string menuPathRuleEngine => "Modify/Delete";
        public override string menuPathToolbox => null;
        public override string tooltip => "Delete GameObjects.";

        public override void run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            foreach (GameObject gameObject in input) {
                if (!alsoDeleteChildren) {
                    foreach (Transform child in gameObject.transform) {
                        child.SetParentSafe(gameObject.transform.parent, true);
                    }
                }
                SceneExtensions.DestroyImmediateSafe(gameObject);
            }
        }
    }
}