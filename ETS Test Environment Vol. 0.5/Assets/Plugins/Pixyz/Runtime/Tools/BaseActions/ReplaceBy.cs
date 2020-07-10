using Pixyz.Tools.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

    public class ReplaceBy : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public enum ReplaceByMode {
            GameObject,
            BoundingBox,
        }

        private bool isReplaceByBB() => replaceBy == ReplaceByMode.BoundingBox;
        private bool isReplaceByGO() => replaceBy == ReplaceByMode.GameObject;

        [UserParameter]
        public ReplaceByMode replaceBy = ReplaceByMode.GameObject;

        [UserParameter("isReplaceByBB")]
        public ReplaceByBoxType boundingBox = ReplaceByBoxType.Oriented;

        [UserParameter("isReplaceByGO")]
        public GameObject gameobject = null;

        [UserParameter(tooltip: "If true, use the rotation of the newly placed object, otherwise, use the rotation of the original object")]
        public bool replaceRotation;

        [UserParameter(tooltip: "If true, use the scale of the newly placed object, otherwise, use the scale of the original object")]
        public bool replaceScale;

        public override int id => 58476964;
        public override int order => 8;
        public override string menuPathRuleEngine => "Optimize/Replace by...";
        public override string menuPathToolbox => "Replace by...";
        public override string tooltip => "Replace each GameObject by another object";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            switch (replaceBy) {
                case ReplaceByMode.BoundingBox:
                    using (new CoreContext(input.ToArray(), false)) {
                        NativeInterface.ReplaceByBox(new NativeInterface.OccurrenceList(new uint[] { 1 }), boundingBox);
                    }
                    break;
                case ReplaceByMode.GameObject:
                    for (int i = 0; i < input.Count; ++i) {

                        var localPosition = input[i].transform.localPosition;
                        var localScale = input[i].transform.localScale;
                        var localRotation = input[i].transform.localRotation;
                        var parent = input[i].transform.parent;

                        SceneExtensions.DestroyImmediateSafe(input[i]);

                        var goRotation = gameobject.transform.localRotation;
                        var goScale = gameobject.transform.localScale;

                        input[i] = SceneExtensions.Instantiate(gameobject);
                        input[i].transform.parent = parent;
                        input[i].transform.localPosition = localPosition;
                        input[i].transform.localRotation = replaceRotation ? goRotation : localRotation;
                        input[i].transform.localScale = replaceScale ? goScale : localScale;
                    }
                    break;
            }
            
            return input;
        }

        public override IList<string> getErrors() {
            var errors = new List<string>();
            if (isReplaceByGO() && gameobject == null) {
                errors.Add("Gameobject field must be set !");
            }
            return errors;
        }
    }
}