using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin {

  public class RandomizeTransform : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public Vector3 rotationMin = new Vector3(0, -180, 0);

        [UserParameter]
        public Vector3 rotationMax = new Vector3(0, 180, 0);

        [UserParameter]
        public Vector3 translationMin = new Vector3(0, 0, 0);

        [UserParameter]
        public Vector3 translationMax = new Vector3(0, 0, 0);

        [UserParameter]
        public Vector3 scaleMin = new Vector3(0.8f, 0.8f, 0.8f);

        [UserParameter]
        public Vector3 scaleMax = new Vector3(1.2f, 1.2f, 1.2f);

        [UserParameter]
        public bool keepPositionTheSame = true;

        public override int id => 9841002;
        public override string menuPathRuleEngine => "Modify/Randomize Transform";
        public override string menuPathToolbox => null;
        public override string tooltip => "Randomize transforms by random values within ranges.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            foreach (GameObject gameObject in input) {
                gameObject.transform.localEulerAngles += new Vector3(UnityEngine.Random.Range(rotationMin.x, rotationMax.x), UnityEngine.Random.Range(rotationMin.y, rotationMax.y), UnityEngine.Random.Range(rotationMin.z, rotationMax.z));
                gameObject.transform.Translate(new Vector3(UnityEngine.Random.Range(translationMin.x, translationMax.x), UnityEngine.Random.Range(translationMin.y, translationMax.y), UnityEngine.Random.Range(translationMin.z, translationMax.z)));
                gameObject.transform.localScale = new Vector3(
                    gameObject.transform.localScale.x * UnityEngine.Random.Range(scaleMin.x, scaleMax.x),
                    gameObject.transform.localScale.y * UnityEngine.Random.Range(scaleMin.y, scaleMax.y),
                    gameObject.transform.localScale.z * UnityEngine.Random.Range(scaleMin.z, scaleMax.z));
            }
            return input;
        }
    }
}