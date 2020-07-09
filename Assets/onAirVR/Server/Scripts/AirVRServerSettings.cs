/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class AirVRServerSettings : ScriptableObject {
    public const string AssetName = "AirVRServerSettings.asset";
    public const string ProjectAssetPath = "Assets/onAirVR/Server/Resources/" + AssetName;

    [SerializeField] private string license = "onairvr.license";
    [SerializeField] private int maxClientCount = 1;
    [SerializeField] private int stapPort = 9090;
    [SerializeField] private bool adaptiveFrameRate = false;
    [SerializeField] [Range(10, 120)] private int minFrameRate = 10;

    // overridable by command line args only
    [SerializeField] private int ampPort;
    [SerializeField] private bool loopbackOnly;
    [SerializeField] private string profiler;

    public bool AdaptiveFrameRate { get { return adaptiveFrameRate; } }
    public int MinFrameRate { get { return minFrameRate; } }
    public int MaxClientCount { get { return maxClientCount; } }
    public string License { get { return license; } }
    public int StapPort { get { return stapPort; } }
    public int AmpPort { get { return ampPort; } }
    public bool LoopbackOnly { get { return loopbackOnly; } }
    public string Profiler { get { return profiler; } }

    public void ParseCommandLineArgs(string[] args) {
        Dictionary<string, string> pairs = parseCommandLine(args);
        if (pairs == null) {
            return;
        }

        string keyConfigFile = "config";
        if (pairs.ContainsKey(keyConfigFile)) {
            if (File.Exists(pairs[keyConfigFile])) {
                try {
                    AirVRServerSettingsReader reader = new AirVRServerSettingsReader();
                    reader.ReadSettings(pairs[keyConfigFile], this);
                }
                catch (Exception e) {
                    Debug.LogWarning("[onAirVR] WARNING: failed to parse " + pairs[keyConfigFile] + " : " + e.ToString());
                }
            }
            pairs.Remove("config");
        }

        foreach (string key in pairs.Keys) {
            if (key.Equals("onairvr_stap_port")) {
                stapPort = parseInt(pairs[key], StapPort,
                    (parsed) => {
                        return 0 <= parsed && parsed <= 65535;
                    },
                    (val) => {
                        Debug.LogWarning("[onAirVR] WARNING: STAP Port number of the command line argument is invalid : " + val);
                    });
            }
            else if (key.Equals("onairvr_amp_port")) {
                ampPort = parseInt(pairs[key], AmpPort,
                    (parsed) => {
                        return 0 <= parsed && parsed <= 65535;
                    },
                    (val) => {
                        Debug.LogWarning("[onAirVR] WARNING: AMP Port number of the command line argument is invalid : " + val);
                    });
            }
            else if (key.Equals("onairvr_loopback_only")) {
                loopbackOnly = pairs[key].Equals("true");
            }
            else if (key.Equals("onairvr_license")) {
                license = pairs[key];
            }
            else if (key.Equals("onairvr_adaptive_frame_rate")) {
                adaptiveFrameRate = pairs[key].Equals("true");
            }
            else if (key.Equals("onairvr_min_frame_rate")) {
                minFrameRate = parseInt(pairs[key], MinFrameRate,
                    (parsed) => {
                        return parsed >= 0;
                    });
            }
            else if (key.Equals("onairvr_profiler")) {
                profiler = pairs[key];
            }
        }
    }

    private Dictionary<string, string> parseCommandLine(string[] args) {
        if (args == null) {
            return null;
        }

        Dictionary<string, string> result = new Dictionary<string, string>();
        for (int i = 0; i < args.Length; i++) {
            int splitIndex = args[i].IndexOf("=");
            if (splitIndex <= 0) {
                continue;
            }

            string name = args[i].Substring(0, splitIndex);
            string value = args[i].Substring(splitIndex + 1);
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) {
                continue;
            }

            result.Add(name, value);
        }
        return result.Count > 0 ? result : null;
    }

    private int parseInt(string value, int defaultValue, Func<int, bool> predicate, Action<string> failed = null) {
        int result;
        if (int.TryParse(value, out result) && predicate(result)) {
            return result;
        }

        if (failed != null) {
            failed(value);
        }
        return defaultValue;
    }

    private float parseFloat(string value, float defaultValue, Func<float, bool> predicate, Action<string> failed = null) {
        float result;
        if (float.TryParse(value, out result) && predicate(result)) {
            return result;
        }

        if (failed != null) {
            failed(value);
        }
        return defaultValue;
    }
}
