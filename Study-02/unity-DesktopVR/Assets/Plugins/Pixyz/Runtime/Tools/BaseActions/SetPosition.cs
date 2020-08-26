using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class SetPosition : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public enum Referential {
            Self,
            Parent,
            World,
        }

        [UserParameter]
        public Referential referential = Referential.Self;

        [UserParameter]
        public Vector3 translation = Vector3.zero;

        [UserParameter]
        public Vector3 rotation = Vector3.zero;

        [UserParameter]
        public Vector3 scale = Vector3.one;

        public override int id => 505047536;
        public override string menuPathRuleEngine => "Set/Position";
        public override string menuPathToolbox => null;
        public override string tooltip => "Set GameObjects' position.\nUse [Self] referential to set a transformation from the current one.\nUse [Parent] referential to set a transformation from the identity matrix in local space.\nUse [World] referential to set a transformation from the identity matrix in world space";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            foreach (GameObject gameObject in input) {
                Transform trf = gameObject.transform;
                if (referential == Referential.Self) {
                    trf.Translate(translation);
                    trf.Rotate(rotation);
                    Vector3 cscale = trf.localScale;
                    trf.localScale = new Vector3(cscale.x * scale.x, cscale.y * scale.y, cscale.z * scale.z);
                } else if (referential == Referential.Parent) {
                    trf.localPosition = translation;
                    trf.localEulerAngles = rotation;
                    trf.localScale = scale;
                } else {
                    // A bit of a cheat, but hey it works
                    Transform parent = trf.parent;
                    trf.SetParentSafe(null);
                    trf.localPosition = translation;
                    trf.localEulerAngles = rotation;
                    trf.localScale = scale;
                    trf.SetParentSafe(parent, true);
                }
            }
            return input;
        }
    }
}