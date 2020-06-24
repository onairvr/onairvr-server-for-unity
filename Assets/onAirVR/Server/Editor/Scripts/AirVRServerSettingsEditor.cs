/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;

class AirVRServerSettingsProvider : SettingsProvider {
    private class Styles {
        public static GUIContent licenseFilePath = new GUIContent("License File");
        public static GUIContent maxClientCount = new GUIContent("Max Client Count");
        public static GUIContent port = new GUIContent("Port");
        public static GUIContent adaptiveFrameRate = new GUIContent("Adaptive Frame Rate");
        public static GUIContent minFrameRate = new GUIContent("Minimum Frame Rate");
    }

    private SerializedObject _settings;
    private SerializedProperty _licenseFilePath;
    private SerializedProperty _maxClientCount;
    private SerializedProperty _port;
    private SerializedProperty _adaptiveFrameRate;
    private SerializedProperty _minFrameRate;

    public AirVRServerSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
        : base(path, scope) { }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
        var settings = AssetDatabase.LoadAssetAtPath<AirVRServerSettings>(AirVRServerSettings.ProjectAssetPath);
        if (settings == null) {
            settings = ScriptableObject.CreateInstance<AirVRServerSettings>();
            AssetDatabase.CreateAsset(settings, AirVRServerSettings.ProjectAssetPath);
            AssetDatabase.SaveAssets();
        }

        _settings = new SerializedObject(settings);
        _licenseFilePath = _settings.FindProperty("license");
        _maxClientCount = _settings.FindProperty("maxClientCount");
        _port = _settings.FindProperty("stapPort");
        _adaptiveFrameRate = _settings.FindProperty("adaptiveFrameRate");
        _minFrameRate = _settings.FindProperty("minFrameRate");
    }

    public override void OnGUI(string searchContext) {
        EditorGUILayout.PropertyField(_licenseFilePath, Styles.licenseFilePath);
        EditorGUILayout.PropertyField(_maxClientCount, Styles.maxClientCount);
        EditorGUILayout.PropertyField(_port, Styles.port);
        EditorGUILayout.PropertyField(_adaptiveFrameRate, Styles.adaptiveFrameRate);
        EditorGUILayout.PropertyField(_minFrameRate, Styles.minFrameRate);

        _settings.ApplyModifiedProperties();
    }

    [SettingsProvider]
    public static SettingsProvider CreateAirVRServerSettingsProvider() {
        var provider = new AirVRServerSettingsProvider("Project/onAirVR Server");
        provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();

        return provider;
    }
}

[CustomEditor(typeof(AirVRServerSettings))]
public class AirVRServerSettingsEditor : Editor {
    public override void OnInspectorGUI() {
        if (GUILayout.Button("Open onAirVR Server Settings...")) {
            SettingsService.OpenProjectSettings("Project/onAirVR Server");
        }
    }
}
