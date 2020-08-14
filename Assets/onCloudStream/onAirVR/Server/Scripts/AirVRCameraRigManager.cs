﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AirVRCameraRigManager : MonoBehaviour {
    public interface EventHandler {
        void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected);
        void AirVRCameraRigActivated(AirVRCameraRig cameraRig);
        void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig);
        void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig);
        void AirVRCameraRigUserDataReceived(AirVRCameraRig cameraRig, byte[] userData);
    }

    private static AirVRCameraRigManager _instanceOnCurrentScene;

    internal static void LoadOncePerScene() {
        if (_instanceOnCurrentScene == null) {
            _instanceOnCurrentScene = FindObjectOfType<AirVRCameraRigManager>();
            if (_instanceOnCurrentScene == null) {
                var go = new GameObject("AirVRCameraRigManager");
                go.AddComponent<AirVRCameraRigManager>();
                Assert.IsTrue(_instanceOnCurrentScene != null);
            }
        }
    }

    internal static void UnloadOncePerScene() {
        if (_instanceOnCurrentScene != null) {
            _instanceOnCurrentScene = null;
        }
    }

    internal static bool CheckIfExistManagerOnCurrentScene() {
        return _instanceOnCurrentScene != null;
    }

    public static AirVRCameraRigManager managerOnCurrentScene {
        get {
            LoadOncePerScene();
            return _instanceOnCurrentScene;
        }
    }

    private AirVRCameraRigList _cameraRigList;

    private AirVRCameraRig notifyCameraRigWillBeBound(int playerID) {
        var config = AirVRClientConfig.Get(playerID);

        var cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAvailableCameraRigs(config.type, cameraRigs);

        AirVRCameraRig selected = null;
        if (Delegate != null) {
            Delegate.AirVRCameraRigWillBeBound(playerID, config, cameraRigs, out selected);
            AirVRClientConfig.Set(playerID, config);
        }
        else if (cameraRigs.Count > 0) {
            selected = cameraRigs[0];
        }
        return selected;
    }

    private void unregisterAllCameraRigs(bool applicationQuit) {
        var cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllRetainedCameraRigs(cameraRigs);

        foreach (var cameraRig in cameraRigs) {
            UnregisterCameraRig(cameraRig, applicationQuit);
        }
    }

    private void updateApplicationTargetFrameRate() {
        if (AirVRServer.settings.AdaptiveFrameRate == false) { return; }

        var cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllRetainedCameraRigs(cameraRigs);

        float maxCameraRigVideoFrameRate = 0.0f;
        foreach (AirVRCameraRig cameraRig in cameraRigs) {
            if (cameraRig.isStreaming) {
                maxCameraRigVideoFrameRate = Mathf.Max(maxCameraRigVideoFrameRate, cameraRig.GetConfig().framerate);
            } 
        }

        Application.targetFrameRate = Mathf.Max(Mathf.RoundToInt(maxCameraRigVideoFrameRate), AirVRServer.settings.MinFrameRate);
    }

    void Awake() {
        if (_instanceOnCurrentScene != null) {
            new UnityException("[ONAIRVR] ERROR: There must exist only one AirVRCameraRigManager at a time.");
        }
        _instanceOnCurrentScene = this;

        _cameraRigList = new AirVRCameraRigList();
        eventDispatcher = new AirVRServerEventDispatcher();
    }

    void Start() {
        var streams = new List<AirVRServerStreamHandover.Streams>();
        AirVRServerStreamHandover.TakeAllStreamsHandedOverInPrevScene(streams);
        foreach (var item in streams) {
            var selected = notifyCameraRigWillBeBound(item.playerID);
            if (selected != null) {
                _cameraRigList.RetainCameraRig(selected);
                selected.BindPlayer(item.playerID, item.mediaStream, item.inputStream);

                if (selected.isStreaming && Delegate != null) {
                    Delegate.AirVRCameraRigActivated(selected);
                }
            }
            else {
                OCSServerPlugin.Disconnect(item.playerID);
            }
        }

        updateApplicationTargetFrameRate();

        eventDispatcher.MessageReceived += onAirVRMessageReceived;
    }

    void Update() {
        OCSServerPlugin.Update();
        eventDispatcher.DispatchEvent();

        var cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllCameraRigs(cameraRigs);
        foreach (var cameraRig in cameraRigs) {
            cameraRig.OnUpdate();
        }
    }

    void LateUpdate() {
        var cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllCameraRigs(cameraRigs);
        foreach (var cameraRig in cameraRigs) {
            cameraRig.OnLateUpdate();
        }
    }

    void OnApplicationQuit() {
        unregisterAllCameraRigs(true);
    }

    void OnDestroy() {
        unregisterAllCameraRigs(false);

        eventDispatcher.MessageReceived -= onAirVRMessageReceived;
        UnloadOncePerScene();
    }

    internal AirVRServerEventDispatcher eventDispatcher { get; private set; }

    internal void RegisterCameraRig(AirVRCameraRig cameraRig) {
        _cameraRigList.AddUnboundCameraRig(cameraRig);
    }

    internal void UnregisterCameraRig(AirVRCameraRig cameraRig, bool applicationQuit = false) {
        _cameraRigList.RemoveCameraRig(cameraRig);

        if (applicationQuit == false && cameraRig.isBoundToClient) {
            cameraRig.PreHandOverStreams();
            AirVRServerStreamHandover.HandOverStreamsForNextScene(new AirVRServerStreamHandover.Streams(cameraRig.playerID, cameraRig.mediaStream, cameraRig.inputStream));

            if (Delegate != null) {
                if (cameraRig.isStreaming) {
                    Delegate.AirVRCameraRigDeactivated(cameraRig);
                }
                Delegate.AirVRCameraRigHasBeenUnbound(cameraRig);
            }
            cameraRig.PostHandOverStreams();
        }
    }

    public EventHandler Delegate { private get; set; }

    // handle AirVRMessages
    private void onAirVRMessageReceived(OCSMessage message) {
        var serverMessage = message as AirVRServerMessage;
        int playerID = serverMessage.source.ToInt32();

        if (serverMessage.IsSessionEvent()) {
            if (serverMessage.Name.Equals(AirVRServerMessage.NameConnected)) {
                onAirVRSessionConnected(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameDisconnected)) {
                onAirVRSessionDisconnected(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameProfilerFrame)) {
                onAirVRProfilerFrameReceived(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameProfilerReport)) {
                onAirVRProfilerReportReceived(playerID, serverMessage);
            }
        }
        else if (serverMessage.IsPlayerEvent()) {
            if (serverMessage.Name.Equals(AirVRServerMessage.NameCreated)) {
                onAirVRPlayerCreated(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameActivated)) {
                onAirVRPlayerActivated(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameDeactivated)) {
                onAirVRPlayerDeactivated(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameDestroyed)) {
                onAirVRPlayerDestroyed(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameShowCopyright)) {
                onAirVRPlayerShowCopyright(playerID, serverMessage);
            }
        }
        else if (message.Type.Equals(OCSMessage.TypeUserData)) {
            onAirVRPlayerUserDataReceived(playerID, serverMessage);
        }
    }

    private void onAirVRSessionConnected(int playerID, AirVRServerMessage message) {
        AirVRServer.NotifyClientConnected(playerID);
    }

    private void onAirVRPlayerCreated(int playerID, AirVRServerMessage message) {
        var selected = notifyCameraRigWillBeBound(playerID);
        if (selected != null) {
            _cameraRigList.RetainCameraRig(selected);
            selected.BindPlayer(playerID);

            OCSServerPlugin.AcceptPlayer(playerID);
        }
        else {
            OCSServerPlugin.Disconnect(playerID);
        }
    }

    private void onAirVRPlayerActivated(int playerID, AirVRServerMessage message) {
        var cameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (cameraRig != null && Delegate != null) {
            Delegate.AirVRCameraRigActivated(cameraRig);
        }

        updateApplicationTargetFrameRate();
    }

    private void onAirVRPlayerDeactivated(int playerID, AirVRServerMessage message) {
        var cameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (cameraRig != null && Delegate != null) {
            Delegate.AirVRCameraRigDeactivated(cameraRig);
        }

        updateApplicationTargetFrameRate();
    }

    private void onAirVRPlayerDestroyed(int playerID, AirVRServerMessage message) {
        var unboundCameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (unboundCameraRig != null) {
            if (unboundCameraRig.isStreaming && Delegate != null) {
                Delegate.AirVRCameraRigDeactivated(unboundCameraRig);
            }

            unboundCameraRig.UnbindPlayer();
            _cameraRigList.ReleaseCameraRig(unboundCameraRig);

            if (Delegate != null) {
                Delegate.AirVRCameraRigHasBeenUnbound(unboundCameraRig);
            }
        }
    }

    private void onAirVRPlayerShowCopyright(int playerID, AirVRServerMessage message) {
        Debug.Log("(C) 2016-2018 onAirVR. All right reserved.");
    }

    private void onAirVRPlayerUserDataReceived(int playerID, AirVRServerMessage message) {
        var cameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (cameraRig != null && Delegate != null) {
            Delegate.AirVRCameraRigUserDataReceived(cameraRig, message.Data_Decoded);
        }
    }

    private void onAirVRSessionDisconnected(int playerID, AirVRServerMessage message) {
        AirVRServer.NotifyClientDisconnected(playerID);
    }

    private void onAirVRProfilerFrameReceived(int playerID, AirVRServerMessage message) {
        //Debug.Log(string.Format("profiler frame: latency: overall {0:0.000} = network {1:0.000} + decode {2:0.000}",
        //                        message.OverallLatency, message.NetworkLatency, message.DecodeLatency));
    }

    private void onAirVRProfilerReportReceived(int playerID, AirVRServerMessage message) {
        Debug.Log(string.Format("profiler report: fps {0:0.0} ({1}/{2:0.000}), avg latency: overall {3:0.000} = network {4:0.000} + decode {5:0.000}",
                                message.FrameCount / message.Duration, message.FrameCount, message.Duration,
                                message.AvgOverallLatency, message.AvgNetworkLatency, message.AvgDecodeLatency));
    }
}
