using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pixyz.Tools {

    [Serializable]
    public sealed class RuleBlock : ISerializationCallbackReceiver {

        [SerializeField]
        public bool isEnabled = true;

        [NonSerialized]
        public bool isRunning = false;

        [SerializeField]
        public int actionId;

        [NonSerialized]
        public Rule rule;

        [NonSerialized]
        public ActionBase _action;
        public ActionBase action {
            get {
                if (_action == null) {
                    _action = ToolsBase.GetRegisteredAction(actionId);
                }
                return _action;
            }
        }

        public void switchEnabled() {
            isEnabled = !isEnabled;
        }

        public void switchDebug() {
            isRunning = !isRunning;
        }

        public RuleBlock(int actionId) {
            this.actionId = actionId;
        }

        public object run(object data) {
            return action.invoke(data);
        }

        public bool isLastBlockInRule => rule.getBlockIndex(this) == rule.blocksCount - 1;

        public void OnBeforeSerialize() {
            serializeParameters();
        }

        public void serializeParameters() {
            if (actionId == 0)
                return;
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            for (int i = 0; i < action.fieldInstances.Length; i++) {
                action.fieldInstances[i].serializeData(ref pairs);
            }
            serializedNames = pairs.Keys.ToArray();
            serializedValues = pairs.Values.ToArray();
        }

        public void OnAfterDeserialize() {

        }

        public void deserializeParameters() {
            if (actionId == 0)
                return;
            Dictionary<string, string> pairs = Enumerable.Range(0, serializedNames.Length).ToDictionary(i => serializedNames[i], i => serializedValues[i]);
            for (int i = 0; i < action.fieldInstances.Length; i++) {
                action.fieldInstances[i].deserializeData(pairs);
            }
        }

        [SerializeField]
        private string[] serializedNames;

        [SerializeField]
        private string[] serializedValues;
    }
}