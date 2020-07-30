/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AirVRServerPackageBuild {
    private const string Version = "2.1.0";

    [MenuItem("onCloudStream/onAirVR/Export onAirVR Server...")]
    public static void Export() {
        string targetPath = EditorUtility.SaveFilePanel("Export onAirVR Server...", "", "onairvr-server_" + Version, "unitypackage");
        if (string.IsNullOrEmpty(targetPath)) { return; }

        if (File.Exists(targetPath)) {
            File.Delete(targetPath);
        }

        string[] assetids = AssetDatabase.FindAssets("", new string[] {
            "Assets/onCloudStream/onAirVR/Server"
        });
        List<string> assets = new List<string>();
        foreach (string assetid in assetids) {
            assets.Add(AssetDatabase.GUIDToAssetPath(assetid));
        }

        AssetDatabase.ExportPackage(assets.ToArray(), targetPath);

        EditorUtility.DisplayDialog("Exported", "Exported successfully.\n\n" + targetPath, "Close");
    }
}
