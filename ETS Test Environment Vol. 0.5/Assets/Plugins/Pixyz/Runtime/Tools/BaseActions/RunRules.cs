using System.Collections.Generic;

namespace Pixyz.Tools.Builtin
{
    public class RunRules : ActionVoid
    {
        [UserParameter(displayName: "")]
        public RuleSet[] ruleSets = new RuleSet[1];

        public override int id { get { return 235363992; } }
        public override string menuPathRuleEngine { get { return "Run Rules"; } }
        public override string menuPathToolbox { get { return null; } }
        public override string tooltip { get { return "Runs specified RuleSets. This allow the nesting of existing RuleSets, to create macro actions. Runs sequentially, from top to bottom."; } }

        public override IList<string> getWarnings()
        {
            List<string> warnings = new List<string>();
            int k = 0;
            foreach (var ruleSet in ruleSets)
            {
                if (ruleSet != null)
                    for (int i = 0; i < ruleSet.rulesCount; i++)
                        for (int j = 0; j < ruleSet.getRule(i).blocksCount; j++)
                            if (ruleSet.getRule(i).getBlock(j).action.id == id)
                                warnings.Add($"RuleSet '{ruleSet.name}' also contains references to other RuleSets. Beware of circular references.");
                if (ruleSet == null)
                    warnings.Add($"The RuleSet {k + 1} is not specified");
                k++;
            }

            return warnings;
        }

        public override void run()
        {
            foreach (var ruleSet in ruleSets)
                ruleSet?.run();
        }
    }
}