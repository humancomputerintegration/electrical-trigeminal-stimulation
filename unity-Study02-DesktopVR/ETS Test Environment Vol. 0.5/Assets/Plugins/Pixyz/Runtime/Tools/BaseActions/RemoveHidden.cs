using Pixyz.Tools.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

#if !MACOS
    public class RemoveHidden : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool runOncePerObject = false;

        [UserParameter]
        public bool considerTransparentOpaque = false;

        [UserParameter]
        public SelectionLevelUnity selectionLevel = SelectionLevelUnity.Triangle;

        public enum SelectionLevelUnity {
            GameObject = 0,
            Submesh = 1,
            Triangle = 2
        }

        public override int id => 65161561;
        public override int order => 9;
        public override string menuPathRuleEngine => "Optimize/Remove Hidden";
        public override string menuPathToolbox => "Remove Hidden";
        public override string tooltip => "Remove hidden geometry.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (runOncePerObject) {
                foreach (GameObject gameObject in input) {
                    using (new CoreContext(input.ToArray(), false)) {
                        NativeInterface.RemoveHidden(new NativeInterface.OccurrenceList(new uint[] { NativeInterface.GetSceneRoot() }), (SelectionLevel)selectionLevel, 1024, 16, 90, considerTransparentOpaque);
                    }
                }
            } else {
                using (new CoreContext(input.ToArray(), false)) {
                    NativeInterface.RemoveHidden(new NativeInterface.OccurrenceList(new uint[] { NativeInterface.GetSceneRoot() }), (SelectionLevel)selectionLevel, 1024, 16, 90, considerTransparentOpaque);
                }
            }

            return input;
        }
    }
#endif
}