using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class AddLight : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public LightType type = LightType.Point;

        [UserParameter]
        public float range = 10f;

        [UserParameter]
        public Color color = Color.white;

        //[UserParameter]
        //public LightmapBakeType mode = LightmapBakeType.Realtime;

        [UserParameter]
        public float intensity = 1f;

        [UserParameter]
        public MonoBehaviour customScript;

        [UserParameter]
        public LightShadows shadowType = LightShadows.Soft;

        public override int id => 11954092;
        public override string menuPathRuleEngine => "Add/Light";
        public override string menuPathToolbox => null;
        public override string tooltip => "Adds Light to GameObjects (if no Light is present).";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            foreach (GameObject gameObject in input) {
                Light light = gameObject.GetOrAddComponent<Light>();
                light = gameObject.AddComponent<Light>();
                light.type = type;
                light.range = range;
                light.intensity = intensity;
                light.shadows = shadowType;
                //light.lightmapBakeType = LightmapBakeType.Realtime;
                light.color = color;
            }
            return input;
        }
    }
}