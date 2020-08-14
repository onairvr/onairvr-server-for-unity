/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public static class OCSServerPlugin {
    private const uint AirVRRenderEventMaskPlayerID = 0xFF000000;
    private const uint AirVRRenderEventMaskArg1 = 0x00FF0000;
    private const uint AirVRRenderEventMaskArg2 = 0x0000FFFF;

    public const string Name = "ocs";
    public const string AudioPluginName = "AudioPlugin_ocs";

    public const int InvalidPlayerID = -1;

    [DllImport(Name)]
    private static extern void ocs_GetOCSServerPluginPtr(ref IntPtr result);

    [DllImport(AudioPluginName)]
    private static extern void ocs_SetOCSServerPluginPtr(IntPtr ptr);

    [DllImport(Name, CharSet = CharSet.Ansi)]
    private static extern int ocs_SetLicenseFile(string filePath);

    [DllImport(Name)]
    private static extern int ocs_Startup(int maxConnectionCount, int portSTAP, int portAMP, bool loopbackOnlyForSTAP, int audioSampleRate);

    [DllImport(Name)]
    private static extern System.IntPtr ocs_Startup_RenderThread_Func();

    [DllImport(Name)]
    private static extern void ocs_Shutdown();

    [DllImport(Name)]
    private static extern IntPtr ocs_Shutdown_RenderThread_Func();

    [DllImport(Name)]
    private static extern bool ocs_GetConfig(int playerID, out IntPtr data, out int length);

    [DllImport(Name)]
    private static extern void ocs_SetConfig(int playerID, string json);

    [DllImport(Name)]
    private static extern void ocs_AcceptPlayer(int playerID);

    [DllImport(Name)]
    private static extern void ocs_Disconnect(int playerID);

    [DllImport(Name)]
    private static extern void ocs_Update();

    [DllImport(Name)]
    private static extern bool ocs_CheckMessageQueue(out IntPtr source, out IntPtr data, out int length);

    [DllImport(Name)]
    private static extern void ocs_RemoveFirstMessage();

    [DllImport(Name)]
    private static extern void ocs_SendUserData(int playerID, IntPtr data, int length);

    [DllImport(Name)]
    private static extern void ocs_EnableProfiler(int profilers);

    [DllImport(Name)]
    private static extern void ocs_SetVideoEncoderParameters(float maxFrameRate, int gopCount);

    [DllImport(Name)]
    private static extern void ocs_RegisterFramebufferTextures(int playerID, IntPtr[] textures, int textureCountPerFrame, int framebufferCount);

    [DllImport(Name)]
    private static extern void ocs_GetViewNumber(int playerID, long timeStamp, float orientationX, float orientationY, float orientationZ, float orientationW, out int viewNumber);

    [DllImport(Name)]
    private static extern void ocs_RecenterPose(int playerID);

    [DllImport(Name)]
    private static extern void ocs_EnableNetworkTimeWarp(int playerID, bool enable);

    [DllImport(Name)]
    private static extern void ocs_SendCameraClipPlanes(int playerID, float nearClip, float farClip);

    [DllImport(Name)]
    private static extern bool ocs_IsStreaming(int playerID);

    [DllImport(Name)]
    private static extern IntPtr ocs_InitStreams_RenderThread_Func();

    [DllImport(Name)]
    private static extern IntPtr ocs_EncodeVideoFrame_RenderThread_Func();

    [DllImport(Name)]
    private static extern IntPtr ocs_ResetStreams_RenderThread_Func();

    [DllImport(Name)]
    private static extern IntPtr ocs_CleanupStreams_RenderThread_Func();


    [DllImport(Name)]
    private static extern void ocs_BeginPendInput(int playerID, ref long timestamp);

    [DllImport(Name)]
    private static extern void ocs_PendInputState(int playerID, byte device, byte control, byte state);

    [DllImport(Name)]
    private static extern void ocs_PendInputRaycastHit(int playerID, byte device, byte control, OCSVector3D origin, OCSVector3D hitPosition, OCSVector3D hitNormal);

    [DllImport(Name)]
    private static extern void ocs_PendInputVibration(int playerID, byte device, byte control, float frequency, float amplitude);

    [DllImport(Name)]
    private static extern void ocs_SendPendingInputs(int playerID, long timestamp);

    [DllImport(Name)]
    private static extern long ocs_GetInputRecvTimestamp(int playerID);

    [DllImport(Name)]
    private static extern bool ocs_GetInputState(int playerID, byte device, byte control, ref byte state);

    [DllImport(Name)]
    private static extern bool ocs_GetInputByteAxis(int playerID, byte device, byte control, ref byte axis);

    [DllImport(Name)]
    private static extern bool ocs_GetInputAxis(int playerID, byte device, byte control, ref float axis);

    [DllImport(Name)]
    private static extern bool ocs_GetInputAxis2D(int playerID, byte device, byte control, ref OCSVector2D axis2D);

    [DllImport(Name)]
    private static extern bool ocs_GetInputPose(int playerID, byte device, byte control, ref OCSVector3D position, ref OCSVector4D rotation);

    [DllImport(Name)]
    private static extern bool ocs_GetInputTouch2D(int playerID, byte device, byte control, ref OCSVector2D position, ref byte state);

    [DllImport(Name)]
    private static extern bool ocs_IsInputActive(int playerID, byte device, byte control);

    [DllImport(Name)]
    private static extern bool ocs_IsInputDirectionActive(int playerID, byte device, byte control, byte direction);

    [DllImport(Name)]
    private static extern bool ocs_GetInputActivated(int playerID, byte device, byte control);

    [DllImport(Name)]
    private static extern bool ocs_GetInputDirectionActivated(int playerID, byte device, byte control, byte direction);

    [DllImport(Name)]
    private static extern bool ocs_GetInputDeactivated(int playerID, byte device, byte control);

    [DllImport(Name)]
    private static extern bool ocs_GetInputDirectionDeactivated(int playerID, byte device, byte control, byte direction);

    [DllImport(Name)]
    private static extern void ocs_UpdateInputFrame(int playerID);

    [DllImport(Name)]
    private static extern void ocs_ClearInput(int playerID);

    [DllImport(AudioPluginName)]
    private static extern void ocs_EncodeAudioFrame(int playerID, float[] data, int sampleCount, int channels, double timestamp);

    [DllImport(AudioPluginName)]
    private static extern void ocs_EncodeAudioFrameForAllPlayers(float[] data, int sampleCount, int channels, double timestamp);

    public static void GetOCSServerPluginPtr(ref IntPtr result) {
        ocs_GetOCSServerPluginPtr(ref result);
    }

    public static void SetOCSServerPluginPtr(IntPtr ptr) {
        ocs_SetOCSServerPluginPtr(ptr);
    }

    public static int SetLicenseFile(string filePath) {
        return ocs_SetLicenseFile(filePath);
    }

    public static int Startup(int maxConnectionCount, int portSTAP, int portAMP, bool loopbackOnlyForSTAP, int audioSampleRate) {
        return ocs_Startup(maxConnectionCount, portSTAP, portAMP, loopbackOnlyForSTAP, audioSampleRate);
    }

    public static IntPtr Startup_RenderThread_Func => ocs_Startup_RenderThread_Func();

    public static void Shutdown() {
        ocs_Shutdown();
    }

    public static IntPtr Shutdown_RenderThread_Func => ocs_Shutdown_RenderThread_Func();

    public static int RenderEventArg(uint playerID, uint data = 0) {
        return (int)((playerID << 24) & AirVRRenderEventMaskPlayerID) + (int)(data & (AirVRRenderEventMaskArg1 | AirVRRenderEventMaskArg2));
    }

    public static int RenderEventArg(uint playerID, uint arg1, uint arg2) {
        return (int)((playerID << 24) & AirVRRenderEventMaskPlayerID) + (int)((arg1 << 16) & AirVRRenderEventMaskArg1) + (int)(arg2 & AirVRRenderEventMaskArg2);
    }

    public static bool GetConfig(int playerID, ref string json) {
        IntPtr data = default;
        int length = 0;

        if (ocs_GetConfig(playerID, out data, out length) == false) { return false; }

        byte[] array = new byte[length];
        Marshal.Copy(data, array, 0, length);
        json = System.Text.Encoding.UTF8.GetString(array, 0, length);
        return true;
    }

    public static void SetConfig(int playerID, string json) {
        ocs_SetConfig(playerID, json);
    }

    public static void AcceptPlayer(int playerID) {
        ocs_AcceptPlayer(playerID);
    }

    public static void Disconnect(int playerID) {
        ocs_Disconnect(playerID);
    }

    public static void Update() {
        ocs_Update();
    }

    public static bool CheckMessageQueue(out IntPtr source, out IntPtr data, out int length) {
        return ocs_CheckMessageQueue(out source, out data, out length);
    }

    public static void RemoveFirstMessage() {
        ocs_RemoveFirstMessage();
    }

    public static void SendUserData(int playerID, IntPtr data, int length) {
        ocs_SendUserData(playerID, data, length);
    }

    public static void EnableProfiler(int profilers) {
        ocs_EnableProfiler(profilers);
    }

    public static void SetVideoEncoderParameters(float maxFrameRate, int gopCount) {
        ocs_SetVideoEncoderParameters(maxFrameRate, gopCount);
    }

    public static void RegisterFramebufferTextures(int playerID, IntPtr[] textures, int textureCountPerFrame, int framebufferCount) {
        ocs_RegisterFramebufferTextures(playerID, textures, textureCountPerFrame, framebufferCount);
    }

    public static void GetViewNumber(int playerID, long timestamp, Quaternion orientation, out int viewNumber) {
        ocs_GetViewNumber(playerID, timestamp, orientation.x, orientation.y, orientation.z, orientation.w, out viewNumber);
    }

    public static void RecenterPose(int playerID) {
        ocs_RecenterPose(playerID);
    }

    public static void EnableNetworkTimeWarp(int playerID, bool enable) {
        ocs_EnableNetworkTimeWarp(playerID, enable);
    }

    public static void SendCameraClipPlanes(int playerID, float nearClip, float farClip) {
        ocs_SendCameraClipPlanes(playerID, nearClip, farClip);
    }

    public static bool IsStreaming(int playerID) {
        return ocs_IsStreaming(playerID);
    }

    public static IntPtr InitStreams_RenderThread_Func => ocs_InitStreams_RenderThread_Func();
    public static IntPtr EncodeVideoFrame_RenderThread_Func => ocs_EncodeVideoFrame_RenderThread_Func();
    public static IntPtr ResetStreams_RenderThread_Func => ocs_ResetStreams_RenderThread_Func();
    public static IntPtr CleanupStreams_RenderThread_Func => ocs_CleanupStreams_RenderThread_Func();

    public static void BeginPendInput(int playerID, ref long timestamp) {
        ocs_BeginPendInput(playerID, ref timestamp);
    }

    public static void PendInputState(int playerID, byte device, byte control, byte state) {
        ocs_PendInputState(playerID, device, control, state);
    }

    public static void PendInputRaycastHit(int playerID, byte device, byte control, Vector3 origin, Vector3 hitPosition, Vector3 hitNormal) {
        ocs_PendInputRaycastHit(playerID, device, control, new OCSVector3D(origin), new OCSVector3D(hitPosition), new OCSVector3D(hitNormal));
    }

    public static void PendInputVibration(int playerID, byte device, byte control, float frequency, float amplitude) {
        ocs_PendInputVibration(playerID, device, control, frequency, amplitude);
    }

    public static void SendPendingInputs(int playerID, long timestamp) {
        ocs_SendPendingInputs(playerID, timestamp);
    }

    public static long GetInputRecvTimestamp(int playerID) {
        return ocs_GetInputRecvTimestamp(playerID);
    }

    public static bool GetInputState(int playerID, byte device, byte control, ref byte state) {
        return ocs_GetInputState(playerID, device, control, ref state);
    }

    public static bool GetInputByteAxis(int playerID, byte device, byte control, ref byte axis) {
        return ocs_GetInputByteAxis(playerID, device, control, ref axis);
    }

    public static bool GetInputAxis(int playerID, byte device, byte control, ref float axis) {
        return ocs_GetInputAxis(playerID, device, control, ref axis);
    }

    public static bool GetInputAxis2D(int playerID, byte device, byte control, ref Vector2 axis2D) {
        var result = new OCSVector2D();

        if (ocs_GetInputAxis2D(playerID, device, control, ref result) == false) { return false; }

        axis2D = result.toVector2();
        return true;
    }

    public static bool GetInputPose(int playerID, byte device, byte control, ref Vector3 position, ref Quaternion rotation) {
        var pos = new OCSVector3D();
        var rot = new OCSVector4D();

        if (ocs_GetInputPose(playerID, device, control, ref pos, ref rot) == false) { return false; }

        position = pos.toVector3();
        rotation = rot.toQuaternion();
        return true;
    }

    public static bool GetInputTouch2D(int playerID, byte device, byte control, ref Vector2 position, ref byte state) {
        var pos = new OCSVector2D();

        if (ocs_GetInputTouch2D(playerID, device, control, ref pos, ref state) == false) { return false; }

        position = pos.toVector2();
        return true;
    }

    public static bool IsInputActive(int playerID, byte device, byte control) {
        return ocs_IsInputActive(playerID, device, control);
    }

    public static bool IsInputDirectionActive(int playerID, byte device, byte control, byte direction) {
        return ocs_IsInputDirectionActive(playerID, device, control, direction);
    }

    public static bool GetInputActivated(int playerID, byte device, byte control) {
        return ocs_GetInputActivated(playerID, device, control);
    }

    public static bool GetInputDirectionActivated(int playerID, byte device, byte control, byte direction) {
        return ocs_GetInputDirectionActivated(playerID, device, control, direction);
    }

    public static bool GetInputDeactivated(int playerID, byte device, byte control) {
        return ocs_GetInputDeactivated(playerID, device, control);
    }

    public static bool GetInputDirectionDeactivated(int playerID, byte device, byte control, byte direction) {
        return ocs_GetInputDirectionDeactivated(playerID, device, control, direction);
    }

    public static void UpdateInputFrame(int playerID) {
        ocs_UpdateInputFrame(playerID);
    }

    public static void ClearInput(int playerID) {
        ocs_ClearInput(playerID);
    }

    public static void EncodeAudioFrame(int playerID, float[] data, int sampleCount, int channels, double timestamp) {
        ocs_EncodeAudioFrame(playerID, data, sampleCount, channels, timestamp);
    }

    public static void EncodeAudioFrameForAllPlayers(float[] data, int sampleCount, int channels, double timestamp) {
        ocs_EncodeAudioFrameForAllPlayers(data, sampleCount, channels, timestamp);
    }
}

public class OCSClientPlugin {
    private const uint RenderEventMaskClearColor = 0x00800000U;
    private const uint RenderEventMaskRenderOnTexture = 0x00400000U;
    private const int RenderEventRenderAspectScale = 1000000;

    public enum FrameType {
        StereoLeft = 0,
        StereoRight,
        Mono
    }

    public enum InputSendPolicy {
        Never = 0,
        Always,
        NonzeroAlwaysZeroOnce,
        OnChange
    }

#if UNITY_ANDROID
    private const string Name = "ocs";

    [DllImport(Name)]
    private static extern void ocs_InitJNI();   // TODO merge name as ocs_InitPlatform()
#elif UNITY_IOS
    private const string Name = "__Internal";

    [DllImport(Name)]
    private static extern int ocs_DeviceRefreshRate();

    [DllImport(Name)]
    private static extern bool ocs_HEVCDecodeSupported();
#else
    private const string Name = "NotImplementedYet";
#endif

    [DllImport(Name)]
    private static extern int ocs_Init(string licenseFilePath, int audioOutputSampleRate, bool hasInput);

    [DllImport(Name)]
    private static extern void ocs_Cleanup();

    [DllImport(Name)]
    private static extern void ocs_EnableCopyrightCheck(bool enable);

    [DllImport(Name)]
    private static extern void ocs_SetProfile(string profile);

    [DllImport(Name)]
    private static extern bool ocs_IsConnected();

    [DllImport(Name)]
    private static extern bool ocs_IsPlaying();

    [DllImport(Name)]
    private static extern void ocs_RequestConnect(string address, int port);

    [DllImport(Name)]
    private static extern void ocs_RequestDisconnect();

    [DllImport(Name)]
    private static extern void ocs_RequestPlay();

    [DllImport(Name)]
    private static extern void ocs_RequestStop();

    [DllImport(Name)]
    private static extern void ocs_SendUserData(IntPtr data, int length);

    [DllImport(Name)]
    private static extern bool ocs_CheckMessageQueue(out IntPtr source, out IntPtr data, out int length);

    [DllImport(Name)]
    private static extern void ocs_RemoveFirstMessageFromQueue();

    [DllImport(Name)]
    private static extern void ocs_EnableNetworkTimeWarp(bool enable);

    [DllImport(Name)]
    private static extern void ocs_SetCameraOrientation(OCSVector4D rotation, ref int viewNumber);

    [DllImport(Name)]
    private static extern void ocs_SetCameraProjection(float left, float top, float right, float bottom);

    [DllImport(Name)]
    private static extern IntPtr ocs_SetRenderAspect_RenderThread_Func();

    [DllImport(Name)]
    private static extern bool ocs_GetVideoRenderTargetTexture(ref IntPtr texture, ref int width, ref int height);

    [DllImport(Name)]
    private static extern IntPtr ocs_PrepareRender_RenderThread_Func();

    [DllImport(Name)]
    private static extern IntPtr ocs_PreRenderVideoFrame_RenderThread_Func();

    [DllImport(Name)]
    private static extern IntPtr ocs_RenderVideoFrame_RenderThread_Func();

    [DllImport(Name)]
    private static extern IntPtr ocs_EndOfRenderFrame_RenderThread_Func();

    [DllImport(Name)]
    private static extern bool ocs_GetAudioData([MarshalAs(UnmanagedType.LPArray)]float[] buffer, int length, int channels);

    [DllImport(Name)]
    private static extern void ocs_BeginPendInput(ref long timestamp);

    [DllImport(Name)]
    private static extern void ocs_PendInputState(byte device, byte control, byte state);

    [DllImport(Name)]
    private static extern void ocs_PendInputByteAxis(byte device, byte control, byte axis);

    [DllImport(Name)]
    private static extern void ocs_PendInputAxis(byte device, byte control, float axis);

    [DllImport(Name)]
    private static extern void ocs_PendInputAxis2D(byte device, byte control, OCSVector2D axis2D);

    [DllImport(Name)]
    private static extern void ocs_PendInputPose(byte device, byte control, OCSVector3D position, OCSVector4D rotation);

    [DllImport(Name)]
    private static extern void ocs_PendInputTouch2D(byte device, byte control, OCSVector2D position, byte state, bool active);

    [DllImport(Name)]
    private static extern void ocs_SendPendingInputs(long timestamp);

    [DllImport(Name)]
    private static extern bool ocs_GetInputState(byte device, byte control, ref byte state);

    [DllImport(Name)]
    private static extern bool ocs_GetInputRaycastHit(byte device, byte control, ref OCSVector3D origin, ref OCSVector3D hitPosition, ref OCSVector3D hitNormal);

    [DllImport(Name)]
    private static extern bool ocs_GetInputVibration(byte device, byte control, ref float frequency, ref float amplitude);

    [DllImport(Name)]
    private static extern void ocs_UpdateInputFrame();

    [DllImport(Name)]
    private static extern void ocs_ClearInput();

    private static float optimisticVideoFrameRate {
        get {
#if UNITY_ANDROID
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject display = activity.Call<AndroidJavaObject>("getSystemService", "window").Call<AndroidJavaObject>("getDefaultDisplay");
            return display.Call<float>("getRefreshRate");
#elif UNITY_IOS
                return ocs_DeviceRefreshRate();
#else
                return 0.0f;
#endif
        }
    }

    private static string[] supportedVideoCodecs {
        get {
            var supportAVC = false;
            var supportHEVC = false;

#if UNITY_ANDROID
            var mediaCodecList = new AndroidJavaObject("android.media.MediaCodecList", 0);
            var mediaCodecInfos = mediaCodecList.Call<AndroidJavaObject[]>("getCodecInfos");
            foreach (var codecInfo in mediaCodecInfos) {
                var types = codecInfo.Call<string[]>("getSupportedTypes");
                foreach (var type in types) {
                    if (type.Equals("video/avc")) {
                        supportAVC = true;
                    }
                    else if (type.Equals("video/hevc")) {
                        supportHEVC = true;
                    }
                }
            }
#elif UNITY_IOS
                supportAVC = true;
                supportHEVC = ocs_HEVCDecodeSupported();
#endif

            Assert.IsTrue(supportHEVC || supportAVC);

            var result = new string[(supportHEVC && supportAVC) ? 2 : 1];
            if (supportHEVC) {
                result[0] = "H265";
            }
            if (supportAVC) {
                result[result.Length - 1] = "H264";
            }
            return result;
        }
    }

    public static void Init() {
        if (Application.isEditor) { return; }

#if UNITY_ANDROID
        // workaround : Only the game module can access to java classes in plugin.
        ocs_InitJNI();
#endif
    }

    public static int Configure(string licenseFilePath, int audioOutputSampleRate, bool hasInput) {
        if (Application.isEditor) { return 0; }

        return ocs_Init(licenseFilePath, audioOutputSampleRate, hasInput);
    }

    public static void Cleanup() {
        if (Application.isEditor) { return; }

        ocs_Cleanup();
    }

    public static void EnableCopyrightCheck(bool enable) {
        if (Application.isEditor) { return; }

        ocs_EnableCopyrightCheck(enable);
    }

    public static void SetProfile(string profile) {
        if (Application.isEditor) { return; }

        ocs_SetProfile(profile);
    }

    public static bool IsConnected() {
        if (Application.isEditor) { return false; }

        return ocs_IsConnected();
    }

    public static bool IsPlaying() {
        if (Application.isEditor) { return false; }

        return ocs_IsPlaying();
    }

    public static void RequestConnect(string address, int port) {
        if (Application.isEditor) { return; }

        ocs_RequestConnect(address, port);
    }

    public static void RequestDisconnect() {
        if (Application.isEditor) { return; }

        ocs_RequestDisconnect();
    }

    public static void RequestPlay() {
        if (Application.isEditor) { return; }

        ocs_RequestPlay();
    }

    public static void RequestStop() {
        if (Application.isEditor) { return; }

        ocs_RequestStop();
    }

    public static void RequestSendUserData(byte[] data) {
        if (Application.isEditor || data == null || data.Length <= 0) { return; }

        var ptr = Marshal.AllocHGlobal(data.Length);
        try {
            Marshal.Copy(data, 0, ptr, data.Length);
            ocs_SendUserData(ptr, data.Length);
        }
        finally {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static bool CheckMessageQueue(out IntPtr source, out IntPtr data, out int length) {
        if (Application.isEditor) {
            source = default;
            data = default;
            length = 0;
            return false;
        }

        return ocs_CheckMessageQueue(out source, out data, out length);
    }

    public static void RemoveFirstMessageFromQueue() {
        if (Application.isEditor) { return; }

        ocs_RemoveFirstMessageFromQueue();
    }

    public static void EnableNetworkTimeWarp(bool enable) {
        if (Application.isEditor) { return; }

        ocs_EnableNetworkTimeWarp(enable);
    }

    public static void SetCameraOrientation(Quaternion rotation, ref int viewNumber) {
        if (Application.isEditor) { return; }

        ocs_SetCameraOrientation(new OCSVector4D(rotation), ref viewNumber);
    }

    public static void SetCameraProjection(float[] projection) {
        if (Application.isEditor) { return; }

        ocs_SetCameraProjection(projection[0], projection[1], projection[2], projection[3]);
    }

    public static void SetRenderAspect(float aspect) {
        if (Application.isEditor) { return; }

        GL.IssuePluginEvent(ocs_SetRenderAspect_RenderThread_Func(), (int)(aspect * RenderEventRenderAspectScale));
    }

    public static bool GetVideoRenderTargetTexture(ref IntPtr texture, ref int width, ref int height) {
        if (Application.isEditor) { return false; }

        return ocs_GetVideoRenderTargetTexture(ref texture, ref width, ref height);
    }

    public static void PrepareRender(bool useSeperateRenderTarget) {
        if (Application.isEditor) { return; }

        GL.IssuePluginEvent(ocs_PrepareRender_RenderThread_Func(), useSeperateRenderTarget ? 1 : 0);
    }

    public static void PreRenderVideoFrame(int viewNumber) {
        if (Application.isEditor) { return; }

        GL.IssuePluginEvent(ocs_PreRenderVideoFrame_RenderThread_Func(), viewNumber);
    }

    public static void RenderVideoFrame(OCSRenderCommand renderCommand, FrameType frameType, bool clearColor, bool renderOnTexture) {
        if (Application.isEditor) { return; }

        renderCommand.Issue(ocs_RenderVideoFrame_RenderThread_Func(), renderEvent(frameType, clearColor, renderOnTexture));
    }

    public static void EndRenderVideoFrame() {
        if (Application.isEditor) { return; }

        GL.IssuePluginEvent(ocs_EndOfRenderFrame_RenderThread_Func(), 0);
    }

    public static bool GetAudioData(float[] buffer, int length, int channels) {
        if (Application.isEditor) { return false; }

        return ocs_GetAudioData(buffer, length, channels);
    }

    public static float GetOptimisticVideoFrameRate() {
        if (Application.isEditor) { return 60.0f; }

        return optimisticVideoFrameRate;
    }

    public static string[] GetSupportedVideoCodecs() {
        if (Application.isEditor) { return new string[] { "H265", "H264" }; }

        return supportedVideoCodecs;
    }

    public static string[] GetSupportedAudioCodecs() {
        return new string[] { "opus" };
    }

    public static void BeginPendInput(ref long timestamp) {
        if (Application.isEditor) { return; }

        ocs_BeginPendInput(ref timestamp);
    }

    public static void PendInputState(byte device, byte control, byte state) {
        if (Application.isEditor) { return; }

        ocs_PendInputState(device, control, state);
    }

    public static void PendInputByteAxis(byte device, byte control, byte axis) {
        if (Application.isEditor) { return; }

        ocs_PendInputByteAxis(device, control, axis);
    }

    public static void PendInputAxis(byte device, byte control, float axis) {
        if (Application.isEditor) { return; }

        ocs_PendInputAxis(device, control, axis);
    }

    public static void PendInputAxis2D(byte device, byte control, Vector2 axis2D) {
        if (Application.isEditor) { return; }

        ocs_PendInputAxis2D(device, control, new OCSVector2D(axis2D));
    }

    public static void PendInputPose(byte device, byte control, Vector3 position, Quaternion rotation) {
        if (Application.isEditor) { return; }

        ocs_PendInputPose(device, control, new OCSVector3D(position), new OCSVector4D(rotation));
    }

    public static void PendInputTouch2D(byte device, byte control, Vector2 position, byte state, bool active) {
        if (Application.isEditor) { return; }

        ocs_PendInputTouch2D(device, control, new OCSVector2D(position), state, active);
    }

    public static void SendPendingInputs(long timestamp) {
        if (Application.isEditor) { return; }

        ocs_SendPendingInputs(timestamp);
    }

    public static bool GetInputState(byte device, byte control, ref byte state) {
        if (Application.isEditor) { return false; }

        return ocs_GetInputState(device, control, ref state);
    }

    public static bool GetInputRaycastHit(byte device, byte control, ref Vector3 origin, ref Vector3 hitPosition, ref Vector3 hitNormal) {
        if (Application.isEditor) { return false; }

        var ori = new OCSVector3D();
        var pos = new OCSVector3D();
        var norm = new OCSVector3D();

        if (ocs_GetInputRaycastHit(device, control, ref ori, ref pos, ref norm) == false) { return false; }

        origin = ori.toVector3();
        hitPosition = pos.toVector3();
        hitNormal = norm.toVector3();
        return true;
    }

    public static bool GetInputVibration(byte device, byte control, ref float frequency, ref float amplitude) {
        return Application.isEditor && ocs_GetInputVibration(device, control, ref frequency, ref amplitude);
    }

    public static void UpdateInputFrame() {
        if (Application.isEditor) { return; }

        ocs_UpdateInputFrame();
    }

    public static void ClearInput() {
        if (Application.isEditor) { return; }

        ocs_ClearInput();
    }

    private static int renderEvent(FrameType frameType, bool clearColor, bool renderOnTexture) {
        return (int)(((int)frameType << 24) + 
                     (clearColor ? RenderEventMaskClearColor : 0) +
                     (renderOnTexture ? RenderEventMaskRenderOnTexture : 0));
    }
}
