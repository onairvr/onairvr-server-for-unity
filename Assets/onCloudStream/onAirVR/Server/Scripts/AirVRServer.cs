/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.IO;
using System.Runtime.InteropServices;

[Serializable]
public class AirVRServerSettingsReader {
#pragma warning disable 414
    [SerializeField] private AirVRServerSettings onairvr;
#pragma warning restore 414

    public void ReadSettings(string fileFrom, AirVRServerSettings to) {
        onairvr = to;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(fileFrom), this);
    }
}

public class AirVRServer : MonoBehaviour {
    private const int StartupErrorNotSupportingGPU = -1;
    private const int StartupErrorLicenseNotYetVerified = -2;
    private const int StartupErrorLicenseFileNotFound = -3;
    private const int StartupErrorInvalidLicenseFile = -4;
    private const int StartupErrorLicenseExpired = -5;

    private const int GroupOfPictures = 0; // use infinite gop by default

    private const int ProfilerFrame = 0x01;
    private const int ProfilerReport = 0x02;

    [DllImport(OCSPlugin.Name)]
    private static extern void ocs_GetOCSServerPluginPtr(ref System.IntPtr result);

    [DllImport(OCSPlugin.AudioPluginName)]
    private static extern void ocs_SetOCSServerPluginPtr(System.IntPtr ptr);

    [DllImport(OCSPlugin.Name, CharSet = CharSet.Ansi)]
    private static extern int ocs_SetLicenseFile(string filePath);

    [DllImport(OCSPlugin.Name)]
    private static extern int ocs_Startup(int maxConnectionCount, int portSTAP, int portAMP, bool loopbackOnlyForSTAP, int audioSampleRate);

    [DllImport(OCSPlugin.Name)]
    private static extern System.IntPtr ocs_Startup_RenderThread_Func();

    [DllImport(OCSPlugin.Name)]
    private static extern void ocs_Shutdown();

    [DllImport(OCSPlugin.Name)]
    private static extern IntPtr ocs_Shutdown_RenderThread_Func();

    [DllImport(OCSPlugin.Name)]
    private static extern void ocs_EnableProfiler(int profilers);

    [DllImport(OCSPlugin.Name)]
    private static extern void ocs_SetVideoEncoderParameters(float maxFrameRate, int gopCount);

    [DllImport(OCSPlugin.AudioPluginName)]
    private static extern void ocs_EncodeAudioFrame(int playerID, float[] data, int sampleCount, int channels, double timestamp);

    [DllImport(OCSPlugin.AudioPluginName)]
    private static extern void ocs_EncodeAudioFrameForAllPlayers(float[] data, int sampleCount, int channels, double timestamp);

    public interface EventHandler {
        void AirVRServerFailed(string reason);
        void AirVRServerClientConnected(int clientHandle);
        void AirVRServerClientDisconnected(int clientHandle);
    }

    private static AirVRServer _instance;
    private static EventHandler _Delegate;

    internal static AirVRServerSettings settings {
        get {
            Assert.IsNotNull(_instance);
            Assert.IsNotNull(_instance._settings);

            return _instance._settings;
        }
    }

    internal static void LoadOnce() {
        if (_instance == null) {
            GameObject go = new GameObject("AirVRServer");
            go.AddComponent<AirVRServer>();
            Assert.IsNotNull(_instance);

            var settings = Resources.Load<AirVRServerSettings>("AirVRServerSettings");
            if (settings == null) {
                settings = ScriptableObject.CreateInstance<AirVRServerSettings>();
            }
            _instance._settings = settings;
            _instance._settings.ParseCommandLineArgs(Environment.GetCommandLineArgs());
        }
    }

    internal static void NotifyClientConnected(int clientHandle) {
        if (_Delegate != null) {
            _Delegate.AirVRServerClientConnected(clientHandle);
        }
    }

    internal static void NotifyClientDisconnected(int clientHandle) {
        if (_Delegate != null) {
            _Delegate.AirVRServerClientDisconnected(clientHandle);
        }
    }

    public static EventHandler Delegate {
        set {
            _Delegate = value;
        }
    }

    public static void SendAudioFrame(AirVRCameraRig cameraRig, float[] data, int sampleCount, int channels, double timestamp) {
        if (cameraRig.isBoundToClient) {
            ocs_EncodeAudioFrame(cameraRig.playerID, data, data.Length / channels, channels, AudioSettings.dspTime);
        }
    }

    public static void SendAudioFrameToAllCameraRigs(float[] data, int sampleCount, int channels, double timestamp) {
        ocs_EncodeAudioFrameForAllPlayers(data, data.Length / channels, channels, AudioSettings.dspTime);
    }

    private bool _startedUp = false;
    private AirVRServerSettings _settings;
    private float _lastTimeEvalFps = 0.0f;
    private int _frameCountSinceLastEvalFps = 0;

    private void Awake() {
        if (_instance != null) {
            new UnityException("[onAirVR] ERROR: There must exist only one AirVRServer instance.");
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        _lastTimeEvalFps = Time.realtimeSinceStartup;

        try {
            Assert.IsNotNull(_settings);

            if (_settings.AdaptiveFrameRate) {
                QualitySettings.vSyncCount = 0;
            }

            ocs_SetLicenseFile(Application.isEditor ? "Assets/onCloudStream/onAirVR/Server/Editor/Misc/onairvr.license" : _settings.License);
            ocs_SetVideoEncoderParameters(120.0f, GroupOfPictures);

            int startupResult = ocs_Startup(_settings.MaxClientCount, _settings.StapPort, _settings.AmpPort, _settings.LoopbackOnly, AudioSettings.outputSampleRate);
            if (startupResult == 0) {   // no error
                System.IntPtr pluginPtr = System.IntPtr.Zero;
                ocs_GetOCSServerPluginPtr(ref pluginPtr);
                ocs_SetOCSServerPluginPtr(pluginPtr);

                GL.IssuePluginEvent(ocs_Startup_RenderThread_Func(), 0);
                _startedUp = true;

                switch (_settings.Profiler) {
                    case "full":
                        ocs_EnableProfiler(ProfilerFrame | ProfilerReport);
                        break;
                    case "frame":
                        ocs_EnableProfiler(ProfilerFrame);
                        break;
                    case "report":
                        ocs_EnableProfiler(ProfilerReport);
                        break;
                    default:
                        break;
                }

                Debug.Log("[onAirVR] INFO: The onAirVR Server has started on port " + _settings.StapPort + ".");
            }
            else {
                string reason;
                switch (startupResult) {
                    case StartupErrorNotSupportingGPU:
                        reason = "Graphic device is not supported";
                        break;
                    case StartupErrorLicenseNotYetVerified:
                        reason = "License is not yet verified";
                        break;
                    case StartupErrorLicenseFileNotFound:
                        reason = "License file not found";
                        break;
                    case StartupErrorInvalidLicenseFile:
                        reason = "Invalid license file";
                        break;
                    case StartupErrorLicenseExpired:
                        reason = "License expired";
                        break;
                    default:
                        reason = "Unknown error occurred";
                        break;
                }

                Debug.LogError("[onAirVR] ERROR: Failed to startup : " + reason);
                if (_Delegate != null) {
                    _Delegate.AirVRServerFailed(reason);
                }
            }
        }
        catch (System.DllNotFoundException) {
            if (_Delegate != null) {
                _Delegate.AirVRServerFailed("Failed to load onAirVR server plugin");
            }
        }
    }

    private void Update() {
        const float evalFpsPeriod = 10.0f;

        if (string.IsNullOrEmpty(_settings.Profiler)) {
            return;
        }

        _frameCountSinceLastEvalFps++;

        var now = Time.realtimeSinceStartup;
        if (_lastTimeEvalFps + evalFpsPeriod < now) {
            var fps = _frameCountSinceLastEvalFps / (now - _lastTimeEvalFps);
            Debug.Log(string.Format("[onAirVR Server] FPS: {0:0.0}", fps));

            _lastTimeEvalFps = now;
            _frameCountSinceLastEvalFps = 0;
        }
    }

    private void OnDestroy() {
        if (_startedUp) {
            GL.IssuePluginEvent(ocs_Shutdown_RenderThread_Func(), 0);
            GL.Flush();

            ocs_Shutdown();
        }
    }
}
