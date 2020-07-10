using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{
    public class FilterOnPosition : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public enum BoundsCondition {
            PivotIsInTheBox,
            InterectsBox,
            IsCompletelyContainedInTheBox
        }

        [UserParameter]
        public Vector3 position;

        [UserParameter]
        public float boxToleranceSize = 1f;

        [UserParameter]
        public BoundsCondition requirement = BoundsCondition.InterectsBox;

        public override int id => 545105221;
        public override string menuPathRuleEngine => "Filter/On Position";
        public override string menuPathToolbox => null;
        public override string tooltip => "Filter input GameObjects based on their position compared to a defined box volume.\nIf GameObject contains a Mesh, it will test the whole mesh againts the box volume, otherwise, it will only use the pivot point of the transform.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            List<GameObject> output = new List<GameObject>();
            Bounds box = new Bounds(position, new Vector3(boxToleranceSize, boxToleranceSize, boxToleranceSize));
            foreach (GameObject gameObject in input) {
                Transform transform = gameObject.transform;
                MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                if (requirement == BoundsCondition.PivotIsInTheBox || renderer == null) {
                    if (box.Contains(transform.position))
                        output.Add(gameObject);
                } else {
                    if (requirement == BoundsCondition.IsCompletelyContainedInTheBox) {
                        if (box.Contains(renderer.bounds.min) && box.Contains(renderer.bounds.max)) {
                            output.Add(gameObject);
                        }
                    } else {
                        if (box.Intersects(renderer.bounds)) {
                            output.Add(gameObject);
                        }
                    }
                }
            }
            return output;
        }
    }
}