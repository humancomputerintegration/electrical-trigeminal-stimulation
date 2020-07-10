using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pixyz.Tools {

    public static class ToolsBase {

        static ToolsBase() {
            Refresh();
        }

        private static Dictionary<int, ActionBase> _Actions;
        public static Dictionary<int, ActionBase> Actions {
            get {
                if (_Actions == null) {
                    _Actions = new Dictionary<int, ActionBase>();
                }
                return _Actions;
            }
        }

        private static Dictionary<KeyValuePair<string, string>, int> _Plugins;
        public static Dictionary<KeyValuePair<string,string>, int> Plugins {
            get {
                if (_Plugins == null) {
                    _Plugins = new Dictionary<KeyValuePair<string, string>, int>();
                }
                return _Plugins;
            }
        }

        /// <summary>
        /// Takes ~30ms to ~200ms depending on platform.
        /// Runs only once per runtime if not refreshed manually.
        /// </summary>
        public static void Refresh() {
            //Profiling.Start("Refresh Toolbox");
            Actions.Clear();
            foreach (Type type in AppDomain.CurrentDomain.GetAllDerivedTypes<ActionBase>()) {
                RegisterAction(type);
            }
            //Profiling.EndAndPrint("Refresh Toolbox");
        }

        public static ActionBase[] GetAllActions() {
            return Actions.Values.ToArray();
        }

        public static ActionBase[] GetPluginsActions() {
            return Plugins.Values.Select(p => Actions[p]).ToArray();
        }

        public static ActionBase GetRegisteredAction(int id) {
            ActionBase output;
            if (!Actions.TryGetValue(id, out output)) {
                //throw new Exception($"Action with id '{id}' haven't been registered.");
                output = new FreePassAction();
                output.initialize();
                return output;
            }
            if (output == null)
                return null;
            // returns a new Instance everytime.
            // would be cleaner storing types in Actions instead of instance.
            output = (ActionBase)Activator.CreateInstance(output.GetType());
            output.initialize();
            return output;
        }

        public static ActionBase GetPluginAction(string pluginName, string functionName) {
            int id;
            if (!Plugins.TryGetValue(new KeyValuePair<string, string>(pluginName, functionName), out id))
            {
                //throw new Exception($"Action with id '{id}' haven't been registered.");
                var output = new FreePassAction();
                output.initialize();
                return output;
            }
            return GetRegisteredAction(id);
        }

        public static ActionBase RegisterAction(Type type) {
            ActionBase ruleEngineAction = (ActionBase)Activator.CreateInstance(type);
            if (Actions.ContainsKey(ruleEngineAction.id)) {
                Debug.LogError($"Actions '{Actions[ruleEngineAction.id].displayNameRuleEngine}' and '{ruleEngineAction.displayNameRuleEngine}' shares the same id '{ruleEngineAction.id}'. Last action won't be registered.");
                return Actions[ruleEngineAction.id];
            }
            ruleEngineAction.initialize(); // Balek ?
            Actions.Add(ruleEngineAction.id, ruleEngineAction);
            return ruleEngineAction;
        }

        public static ActionBase[] GetAvailableActions(ActionBase previousAction, ActionBase nextAction) {
            return Actions.Values
                // If no previous action, keep only actions without inputs, otherwise, picks thoses that match input with previous action's output
                .Where(x => (previousAction != null) ? IsSubclassOrIsEqual(x.inputType, previousAction.outputType) : x.inputType == null)
                // If no next action, keep all actions, otherwise, picks thoses that match output with next actions's input
                .Where(x => (nextAction != null) ? IsSubclassOrIsEqual(nextAction.inputType, x.outputType) : true)
                .ToArray();
        }

        public static bool CanFitWithin(ActionBase action, ActionBase previousAction, ActionBase nextAction) {
            if (previousAction == null)
                return false;
            if (nextAction == null) {
                return IsSubclassOrIsEqual(action.inputType, previousAction.outputType);
            } else {
                return IsSubclassOrIsEqual(action.inputType, previousAction.outputType)
                    && IsSubclassOrIsEqual(nextAction.inputType, action.outputType);
            }
        }

        public static bool IsSubclassOrIsEqual(Type typeA, Type typeB) {
            if (typeA == typeB)
                return true;
            if (typeA == null || typeB == null) {
                return false;
            }
            return typeA.IsSubclassOf(typeB);
        }
    }
}