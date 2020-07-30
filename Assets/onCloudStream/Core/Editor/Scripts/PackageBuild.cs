/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class OCSCorePackageBuild  {
    private const string Version = "2.1.0";

    [MenuItem("onCloudStream/Core/Export...")]
    public static void Export() {
        string targetPath = EditorUtility.SaveFilePanel("Export onCloudStream Core...", "", "ocs-core_" + Version, "unitypackage");
        if (string.IsNullOrEmpty(targetPath)) { return; }

        if (File.Exists(targetPath)) {
            File.Delete(targetPath);
        }

        var assetids = AssetDatabase.FindAssets("", new string[] {
            "Assets/onCloudStream/Core"
        });
        var assets = new List<string>();
        foreach (var assetid in assetids) {
            assets.Add(AssetDatabase.GUIDToAssetPath(assetid));
        }

        AssetDatabase.ExportPackage(assets.ToArray(), targetPath);

        EditorUtility.DisplayDialog("Exported", "Exported successfully.\n\n" + targetPath, "Close");
    }
}
