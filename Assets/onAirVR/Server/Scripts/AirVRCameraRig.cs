﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Runtime.InteropServices;

[ExecuteInEditMode]
public abstract class AirVRCameraRig : MonoBehaviour {
    private const int InvalidPlayerID = -1;

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_GetViewNumber(int playerID, long timeStamp, float orientationX, float orientationY, float orientationZ, float orientationW, out int viewNumber);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern IntPtr ocs_InitStreams_RenderThread_Func();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern IntPtr ocs_EncodeVideoFrame_RenderThread_Func();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern IntPtr ocs_ResetStreams_RenderThread_Func();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern IntPtr ocs_CleanupStreams_RenderThread_Func();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_IsStreaming(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_RecenterPose(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_EnableNetworkTimeWarp(int playerID, bool enable);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_SendCameraClipPlanes(int playerID, float nearClip, float farClip);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_SendUserData(int playerID, IntPtr data, int length);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_Disconnect(int playerID);

    private Pose _cameraPose = Pose.identity;
    private AirVRClientConfig _config;
    private bool _mediaStreamJustStopped;
    private int _viewNumber;
    private bool _encodeVideoFrameRequested;

    private void enableCameras() {
        foreach (Camera cam in cameras) {
            cam.enabled = true;
        }
    }

    private void disableCameras() {
        foreach (Camera cam in cameras) {
            cam.enabled = false;
        }
    }

    private void initializeCamerasForMediaStream() {
        Assert.IsNotNull(_config);

        setupCamerasOnBound(_config);
    }

    private void startToRenderCamerasForMediaStream() {
        enableCameras();
        onStartRender();

        _mediaStreamJustStopped = false;
        StartCoroutine(CallPluginEndOfFrame());
    }

    private Transform findDirectChildByName(Transform parent, string name) {
        Transform[] xforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform xform in xforms) {
            if (xform.parent == parent && xform.gameObject.name.Equals(name)) {
                return xform;
            }
        }
        return null;
    }

    private void Awake() {
        ensureGameObjectIntegrity();
        if (Application.isPlaying == false) {
            return;
        }

        AirVRServer.LoadOnce();

        disableCameras();
        AirVRCameraRigManager.managerOnCurrentScene.RegisterCameraRig(this);
        AirVRCameraRigManager.managerOnCurrentScene.eventDispatcher.MessageReceived += onAirVRMessageReceived;

        playerID = InvalidPlayerID;

        inputStream = new AirVRServerInputStream(this);

        onAwake();
    }

    private void Start() {
        ensureGameObjectIntegrity();
        if (Application.isPlaying == false) {
            return;
        }

        onStart();
    }

    private void Update() {
        ensureGameObjectIntegrity();
        if (Application.isPlaying == false) {
            return;
        }
    }

    // Events called by AirVRCameraRigManager to guarantee the update execution order
    internal void OnUpdate() {
        inputStream.UpdateInputFrame();
    }

    internal void OnLateUpdate() {
        if (mediaStream != null && _mediaStreamJustStopped == false && _encodeVideoFrameRequested) {
            Assert.IsTrue(isBoundToClient);

            long timestamp = inputStream.inputRecvTimestamp;
            _cameraPose = inputStream.GetPose((byte)AirVRInputDeviceID.HeadTracker, (byte)AirVRHeadTrackerControl.Pose);

            ocs_GetViewNumber(playerID, timestamp, _cameraPose.rotation.x, _cameraPose.rotation.y, _cameraPose.rotation.z, _cameraPose.rotation.w, out _viewNumber);
            updateCameraTransforms(_config, _cameraPose.position, _cameraPose.rotation);
            updateControllerTransforms(_config);

            mediaStream.GetNextFramebufferTexturesAsRenderTargets(cameras);
        }
        inputStream.UpdateSenders();
    }

    private void OnDestroy() {
        if (Application.isPlaying == false) {
            return;
        }

        if (AirVRCameraRigManager.CheckIfExistManagerOnCurrentScene()) {
            AirVRCameraRigManager.managerOnCurrentScene.eventDispatcher.MessageReceived -= onAirVRMessageReceived;
            AirVRCameraRigManager.managerOnCurrentScene.UnregisterCameraRig(this);
        }
    }

    private void onAirVRMessageReceived(AirVRMessage message) {
        AirVRServerMessage serverMessage = message as AirVRServerMessage;
        int srcPlayerID = serverMessage.source.ToInt32();
        if (srcPlayerID != playerID) {
            return;
        }

        if (serverMessage.IsMediaStreamEvent()) {
            if (serverMessage.Name.Equals(AirVRServerMessage.NameInitialized)) {
                onAirVRMediaStreamInitialized(serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameStarted)) {
                onAirVRMediaStreamStarted(serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameEncodeVideoFrame)) {
                onAirVRMediaStreamEncodeVideoFrame(serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameSetCameraProjection)) {
                onAirVRMediaStreamSetCameraProjection(serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameStopped)) {
                onAirVRMediaStreamStopped(serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameCleanupUp)) {
                onAirVRMediaStreamCleanedUp(serverMessage);
            }
        }
    }

    private void onAirVRMediaStreamInitialized(AirVRServerMessage message) {
        Assert.IsNull(mediaStream);

        initializeCamerasForMediaStream();
        ocs_SendCameraClipPlanes(playerID, cameras[0].nearClipPlane, cameras[0].farClipPlane);

        mediaStream = new AirVRServerMediaStream(playerID, _config, cameras.Length);
        GL.IssuePluginEvent(ocs_InitStreams_RenderThread_Func(), AirVRServerPlugin.RenderEventArg((uint)playerID));

        inputStream.Init();
    }

    private void onAirVRMediaStreamStarted(AirVRServerMessage message) {
        startToRenderCamerasForMediaStream();
        inputStream.Start();
    }

    private void onAirVRMediaStreamEncodeVideoFrame(AirVRServerMessage message) {
        _encodeVideoFrameRequested = true;
    }

    private void onAirVRMediaStreamSetCameraProjection(AirVRServerMessage message) {
        updateCameraProjection(_config, message.CameraProjection);
    }

    private void onAirVRMediaStreamStopped(AirVRServerMessage message) {
        onStopRender();
        disableCameras();

        _mediaStreamJustStopped = true; // StopCoroutine(_CallPluginEndOfFrame) executes the routine one more in the next frame after the call. 
                                        // so use a flag to completely stop the routine.

        GL.IssuePluginEvent(ocs_ResetStreams_RenderThread_Func(), AirVRServerPlugin.RenderEventArg((uint)playerID));

        inputStream.Stop();
    }

    private void onAirVRMediaStreamCleanedUp(AirVRServerMessage message) {
        Assert.IsTrue(_mediaStreamJustStopped);
        Assert.IsNotNull(mediaStream);

        inputStream.Cleanup();

        GL.IssuePluginEvent(ocs_CleanupStreams_RenderThread_Func(), AirVRServerPlugin.RenderEventArg((uint)playerID));

        mediaStream.Destroy();
        mediaStream = null;

        foreach (Camera cam in cameras) {
            cam.targetTexture = null;
        }
    }

    private IEnumerator CallPluginEndOfFrame() {
        yield return new WaitForEndOfFrame();

        Assert.IsNotNull(mediaStream);
        GL.IssuePluginEvent(ocs_EncodeVideoFrame_RenderThread_Func(), AirVRServerPlugin.RenderEventArg((uint)playerID, (uint)_viewNumber, (uint)mediaStream.currentFramebufferIndex));    // the first render event

        while (_mediaStreamJustStopped == false) {
            yield return new WaitForEndOfFrame();

            if (_mediaStreamJustStopped) {
                yield break;
            }
            else if (_encodeVideoFrameRequested) {
                Assert.IsNotNull(mediaStream);

                GL.IssuePluginEvent(ocs_EncodeVideoFrame_RenderThread_Func(), AirVRServerPlugin.RenderEventArg((uint)playerID, (uint)_viewNumber, (uint)mediaStream.currentFramebufferIndex));
                _encodeVideoFrameRequested = false;
            }
        }
    }

    protected Transform getOrCreateGameObject(string name, Transform parent) {
        Transform result = findDirectChildByName(parent, name);
        if (result == null) {
            result = new GameObject(name).transform;
            result.parent = parent;
            result.localPosition = Vector3.zero;
            result.localRotation = Quaternion.identity;
            result.localScale = Vector3.one;
        }
        return result;
    }

    protected abstract void ensureGameObjectIntegrity();
    protected virtual void onAwake() { }
    protected virtual void onStart() { }
    protected abstract void setupCamerasOnBound(AirVRClientConfig config);
    protected virtual void onStartRender() { }
    protected virtual void onStopRender() { }
    protected abstract void updateCameraProjection(AirVRClientConfig config, float[] projection);
    protected abstract void updateCameraTransforms(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation);
    protected virtual void updateControllerTransforms(AirVRClientConfig config) { }

    internal int playerID { get; private set; }

    internal AirVRServerInputStream inputStream { get; private set; }
    public AirVRServerMediaStream mediaStream { get; private set; }

    internal bool isStreaming {
        get {
            return ocs_IsStreaming(playerID);
        }
    }

    internal abstract bool raycastGraphic { get; }
    internal abstract Matrix4x4 clientSpaceToWorldMatrix { get; }
    internal abstract Transform headPose { get; }
    internal abstract Camera[] cameras { get; }

    internal void BindPlayer(int playerID) {
        Assert.IsFalse(isBoundToClient);
        Assert.IsNull(_config);

        this.playerID = playerID;
        _config = AirVRServerPlugin.GetConfig(playerID);

        Assert.IsNotNull(_config);
    }

    internal void BindPlayer(int playerID, AirVRServerMediaStream mediaStream, AirVRServerInputStream inputStream) {
        BindPlayer(playerID);

        this.mediaStream = mediaStream;
        this.inputStream = inputStream;
        this.inputStream.owner = this;

        initializeCamerasForMediaStream();
        if (isStreaming) {
            startToRenderCamerasForMediaStream();
        }
    }

    internal void UnbindPlayer() {
        Assert.IsTrue(isBoundToClient);

        playerID = InvalidPlayerID;
        _config = null;
    }

    internal void PreHandOverStreams() {
        Assert.IsTrue(isBoundToClient);

        // do nothing
    }

    internal void PostHandOverStreams() {
        foreach (Camera cam in cameras) {
            cam.targetTexture = null;
        }
    }

    internal void EnableNetworkTimeWarp(bool enable) {
        if (isBoundToClient) {
            ocs_EnableNetworkTimeWarp(playerID, enable);
        }
    }

    public AirVRClientType type {
        get {
            return GetType() == typeof(AirVRStereoCameraRig) ? AirVRClientType.Stereoscopic : AirVRClientType.Monoscopic;
        }
    }

    public bool isBoundToClient {
        get {
            return playerID >= 0;
        }
    }

    public bool isActive {
        get {
            return isBoundToClient && isStreaming;
        }
    }

    public AirVRClientConfig GetConfig() {
        if (isBoundToClient) {
            return _config;
        }
        return null;
    }

    public void RecenterPose() {
        if (isBoundToClient) {
            ocs_RecenterPose(playerID);
        }
    }

    public void SendUserData(byte[] data) {
        if (isBoundToClient) {
            IntPtr ptr = Marshal.AllocHGlobal(data.Length);

            try {
                Marshal.Copy(data, 0, ptr, data.Length);
                ocs_SendUserData(playerID, ptr, data.Length);
            }
            finally {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }

    public void Disconnect() {
        if (isBoundToClient) {
            ocs_Disconnect(playerID);
        }
    }
}
