using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProxyOcclusionCuller))]
public class ProxyOcclusionCullerEditor : Editor {

    public ProxyOcclusionCuller fastPointCloud => (ProxyOcclusionCuller)target;

    public override void OnInspectorGUI() {

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Voxelization", EditorStyles.boldLabel);
        fastPointCloud.voxelSize = EditorGUILayout.FloatField("Voxel Size", fastPointCloud.voxelSize);
        fastPointCloud.mirrorX = EditorGUILayout.Toggle("Mirror X", fastPointCloud.mirrorX);
        fastPointCloud.isZup = EditorGUILayout.Toggle("Is Z Up", fastPointCloud.isZup);
        fastPointCloud.hashedColors = EditorGUILayout.Toggle("Use Hashed Colors", fastPointCloud.hashedColors);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Buffer", EditorStyles.boldLabel);
        fastPointCloud.scalingMode = (ProxyOcclusionCuller.ScalingMode)EditorGUILayout.EnumPopup("Scaling Mode", fastPointCloud.scalingMode);
        if (fastPointCloud.scalingMode == ProxyOcclusionCuller.ScalingMode.Variable) {
            fastPointCloud.scaleFactor = EditorGUILayout.Slider("Scale", fastPointCloud.scaleFactor, 0.1f, 1f);
        } else {
            fastPointCloud.resolution = EditorGUILayout.IntField("Resolution", fastPointCloud.resolution);
        }
        fastPointCloud.ratioMode = (ProxyOcclusionCuller.RatioMode)EditorGUILayout.EnumPopup("Ratio Mode", fastPointCloud.ratioMode);
        fastPointCloud.maximumReadbackRequests = EditorGUILayout.IntSlider("Maximum Readback Requests", fastPointCloud.maximumReadbackRequests, 1, 10);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);
        fastPointCloud.debugBuffer = EditorGUILayout.Toggle("Show Buffer", fastPointCloud.debugBuffer);
        fastPointCloud.debugBounds = EditorGUILayout.Toggle("Show Bounds", fastPointCloud.debugBounds);
        if (fastPointCloud._debugStats = EditorGUILayout.Toggle("Show Stats", fastPointCloud._debugStats)) {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Visible Renderers", fastPointCloud.visibleCells + " / " + fastPointCloud.renderersCount);
            EditorGUILayout.LabelField("Renderers Identification", fastPointCloud.idenficationTime + " ms");
            EditorGUILayout.LabelField("Renderers Activation", fastPointCloud.activationTime + " ms");
            EditorGUILayout.LabelField("Buffer Resolution", fastPointCloud.textureWidth + " x " + fastPointCloud.textureHeight);
            EditorGUILayout.ObjectField("Proxy Material", fastPointCloud.proxyMaterial, typeof(Material), false);
            EditorGUI.indentLevel--;
            EditorUtility.SetDirty(target);
        }

        if (EditorGUI.EndChangeCheck()) {
            fastPointCloud.Initialize();
        }

        if (GUILayout.Button("Save Buffer as .png")) {
            fastPointCloud.saveTo = EditorUtility.SaveFilePanel("Save as *.png", "", "buffer", "png");
        }
    }

    public override void OnPreviewGUI(Rect rect, GUIStyle background) {

    }

    public override bool HasPreviewGUI() {
        return false;
    }
}