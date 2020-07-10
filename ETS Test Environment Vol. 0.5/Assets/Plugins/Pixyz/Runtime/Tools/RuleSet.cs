using Pixyz.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Tools
{

    /// <summary>
    /// A RuleEngine set of Rules.
    /// </summary>
    public sealed class RuleSet : ScriptableObject {

        /// <summary>
        /// Triggered when a property has been changed.
        /// </summary>
        public VoidHandler changed;

        public void invokeChanged() {
            changed?.Invoke();
        }

        /// <summary>
        /// Callback function triggered everytime the importer has progressed.
        /// Always occurs in the main thread.
        /// </summary>
        public ProgressHandler progressed;

        [SerializeField]
        private List<Rule> rules = new List<Rule>();

        public void run() {

            Profiling.Start("RuleEngine");

            progressed?.Invoke(0, "Initializing...");

            int total = getTotalBlocksCount();
            int current = 0;
            try {
                for (int r = 0; r < rules.Count; r++) {
                    Rule rule = rules[r];
                    if (!rule.isEnabled)
                        continue;
                    object data = null;
                    for (int b = 0; b < rule.blocksCount; b++) {
                        RuleBlock block = rule.getBlock(b);
                        progressed?.Invoke(1f * current++ / total, $"{rule.name} > {block.action.displayNameRuleEngine} ({current}/{total})");
                        data = block.run(data);
                    }
                }
                progressed?.Invoke(1f, "Done !");
            } catch (Exception exception) {
                Debug.LogError("Rule Engine Exception : " + exception);
                progressed?.Invoke(1f, "Failure !");
            }
        }

        public int getTotalBlocksCount() {
            int total = 0;
            for (int r = 0; r < rules.Count; r++) {
                for (int b = 0; b < rules[r].blocksCount; b++) {
                    total++;
                }
            }
            return total;
        }

        private void OnEnable() {
            for (int r = 0; r < rules.Count; r++) {
                for (int b = 0; b < rules[r].blocksCount; b++) {
                    rules[r].getBlock(b).deserializeParameters();
                }
            }
        }

        public Rule getRule(int i) => rules[i];

        public void setRule(int i, Rule rule) => rules[i] = rule;

        public int getRuleIndex(Rule rule) => rules.IndexOf(rule);

        public void removeRuleAt(int index) => rules.Remove(rules[index]);

        public void removeRule(Rule rule) => rules.Remove(rule);

        public void appendRule(Rule rule) => rules.Add(rule);

        public int rulesCount => rules.Count;
    }
}
