using Pixyz.Tools.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

    public class CreateLightmapUVs : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public TextureQuality quality;

        [UserParameter]
        public int resolution = 1024;

        [UserParameter]
        public int padding = 2;

        public override int id => 1651980;
        public override int order => 5;
        public override string menuPathRuleEngine => "Optimize/Create UVs for Lightmaps";
        public override string menuPathToolbox => "Create UVs for Lightmaps";
        public override string tooltip => "Generates UVs for Unity lightmapping on the channel 1.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            using (new CoreContext(input.ToArray(), false)) {
                NativeInterface.CreateLightmapUVs(resolution, padding);
            }

            return input;
        }

        private TextureQuality previousQuality;
        private int previousResolution;

        public override void onBeforeDraw() {
            base.onBeforeDraw();
            BaseExtensions.MatchEnumWithCustomValue(ref previousQuality, ref quality, ref previousResolution, ref resolution);
        }

        public override IList<string> getErrors() {
            var errors = new List<string>();
            if (padding > 100) {
                errors.Add("Padding is too high ! (must be between 1 and 100)");
            }
            if (padding < 1) {
                errors.Add("Padding resolution is too low ! (must be between 1 and 100)");
            }
            if (resolution < 64) {
                errors.Add("Resolution is too low ! (must be between 64 and 8192)");
            }
            if (resolution > 8192) {
                errors.Add("Resolution is too high ! (must be between 64 and 8192)");
            }
            return errors.ToArray();
        }
    }
}