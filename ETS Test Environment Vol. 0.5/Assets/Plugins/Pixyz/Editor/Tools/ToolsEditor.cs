using Pixyz.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Tools.Editor {

    public static class ToolsEditor {

        public static void CreateNewAction(bool isInRuleEngine, bool isInToolbox) {

            string path = Path.Combine(Preferences.PluginLocation, "Editor");

            Directory.CreateDirectory(path);

            string id = UnityEngine.Random.Range(0, 999999999).ToString();
            string className = "NewCustomAction_" + id;
            string assetPathAndName = path + "/" + className + ".cs";

            Debug.Log($"Custom Action created : '{assetPathAndName}'");

            File.WriteAllText(assetPathAndName,
@"using System.Collections.Generic;
using UnityEngine;
using Pixyz.Tools;

public class " + className + @" : ActionInOut<IList<GameObject>, IList<GameObject>> {

    [UserParameter]
    public float aFloatParameter = 1f;

    [UserParameter]
    public string aStringParameter = ""Text"";

    [HelperMethod]
    public void resetParameters() {
        aFloatParameter = 1f;
        aStringParameter = ""Text"";
    }

    public override int id { get { return " + id + ";} }" + @"
    public override string menuPathRuleEngine { get { return " + (isInRuleEngine ? "\"Custom/New Custom Action\"" : "null") + ";} }" + @"
    public override string menuPathToolbox { get { return " + (isInToolbox ? "\"Custom/New Custom Action\"" : "null") + ";} }" + @"
    public override string tooltip { get { return ""A Custom Action"";} }

    public override IList<GameObject> run(IList<GameObject> input) {
        /// Your code here
        return input;
    }
}");

            if (isInToolbox)
                Toolbox.RefreshMenus();
            else
                AssetDatabase.Refresh();
        }
    }
}