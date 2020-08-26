using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class AddRigidbody : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public float mass = 1f;

        [UserParameter]
        public float drag = 0f;

        [UserParameter]
        public float angularDrag = 0.05f;

        [UserParameter]
        public bool useGravity = true;

        [UserParameter]
        public bool isKinematic = false;

        [UserParameter]
        public RigidbodyInterpolation interpolate = RigidbodyInterpolation.None;

        [UserParameter]
        public CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;

        public override int id => 643180522;
        public override string menuPathRuleEngine => "Add/Rigidbody";
        public override string menuPathToolbox => null;
        public override string tooltip => "Adds Rigidbody to GameObjects (if no Rigidbody is present)";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            foreach (GameObject gameObject in input) {
                Rigidbody rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
                rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.mass = mass;
                rigidbody.drag = drag;
                rigidbody.angularDrag = angularDrag;
                rigidbody.useGravity = useGravity;
                rigidbody.isKinematic = isKinematic;
                rigidbody.interpolation = interpolate;
                rigidbody.collisionDetectionMode = collisionDetection;
            }
            return input;
        }
    }
}