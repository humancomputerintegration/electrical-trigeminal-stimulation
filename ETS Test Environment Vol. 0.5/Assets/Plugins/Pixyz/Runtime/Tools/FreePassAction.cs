using Pixyz.Config;

namespace Pixyz.Tools {

    public class FreePassAction : ActionInOut<object, object> {

        public override int id => 0;
        public override string menuPathRuleEngine => "MISSING ACTION";
        public override string menuPathToolbox => "MISSING ACTION";
        public override string tooltip => "This Action ID does not exists in your current Unity project. Add this Action in your project or replace this block with an existing Action. This block's output is equal to the given input.";

        public override object run(object input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();
            return input;
        }
    }
}