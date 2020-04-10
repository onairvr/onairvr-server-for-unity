/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class AirVRServerInputStream : AirVRInputStream {
    [DllImport(AirVRServerPlugin.Name)]
    private static extern byte ocs_RegisterInputDeviceFeedback(int playerID, string name, IntPtr cookieTexture, int cookieTextureSize = 0, float cookieDepthScaleMultiplier = 0);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_UnregisterInputSender(int playerID, byte id);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern long ocs_GetInputRecvTimestamp(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_BeginPendInput(int playerID, ref long timestamp);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_PendInputRaycastHit(int playerID, byte deviceID, byte controlID,
                                                       float worldRayOriginX, float worldRayOriginY, float worldRayOriginZ,
                                                       float worldHitPositionX, float worldHitPositionY, float worldHitPositionZ,
                                                       float worldHitNormalX, float worldHitNormalY, float worldHitNormalZ, byte policy);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_PendInputByte(int playerID, byte deviceID, byte controlID, byte value, byte policy);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputTransform(int playerID, byte deviceID, byte controlID,
                                                     ref float posX, ref float posY, ref float posZ, 
                                                     ref float rotX, ref float rotY, ref float rotZ, ref float retW);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputFloat4(int playerID, byte deviceID, byte controlID, ref float x, ref float y, ref float z, ref float w);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputFloat3(int playerID, byte deviceID, byte controlID, ref float x, ref float y, ref float z);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputFloat2(int playerID, byte deviceID, byte controlID, ref float x, ref float y);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputFloat(int playerID, byte deviceID, byte controlID, ref float value);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_SendPendingInputs(int playerID, long timestamp);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_ResetInput(int playerID);

    public AirVRCameraRig owner { get; set; }

    public override void Init() {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        foreach (var key in senders.Keys) {
            var feedback = senders[key] as AirVRInputDeviceFeedback;
            Assert.IsNotNull(feedback);

            registerInputDeviceFeedback(feedback);
        }

        base.Init();
    }

    public void AddInputDevice(AirVRInputDevice device) {
        receivers.Add(device.name, device);
    }

    public void AddInputDeviceFeedback(AirVRInputDeviceFeedback feedback) {
        senders.Add(feedback.name, feedback);
    }

    public bool GetTransform(string deviceName, byte controlID, ref Vector3 position, ref Quaternion orientation) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetTransform(controlID, ref position, ref orientation);
        }
        return false;
    }

    public Quaternion GetOrientation(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetOrientation(controlID);
        }
        return Quaternion.identity;
    }

    public Vector2 GetAxis2D(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetAxis2D(controlID);
        }
        return Vector2.zero;
    }

    public float GetAxis(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetAxis(controlID);
        }
        return 0.0f;
    }

    public float GetButtonRaw(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButtonRaw(controlID);
        }
        return 0.0f;
    }

    public bool GetButton(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButton(controlID);
        }
        return false;
    }

    public bool GetButtonDown(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButtonDown(controlID);
        }
        return false;
    }

    public bool GetButtonUp(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButtonUp(controlID);
        }
        return false;
    }

    public int GetTouchCount(string deviceName) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetTouchCount();
        }
        return 0;
    }

    public AirVRInput.Touch GetTouch(string deviceName, int index) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetTouch(index);
        }
        return null;
    }

    public void RequestVibrate(string deviceName, AirVRHapticVibration vibration) {
        if (senders.ContainsKey(deviceName) == false) { return; }

        var feedback = senders[deviceName] as AirVRInputDeviceFeedback;
        if (feedback != null) {
            feedback.PendVibrate(vibration);
        }
    }

    public bool CheckIfInputDeviceAvailable(string deviceName) {
        return receivers.ContainsKey(deviceName) && receivers[deviceName].isRegistered;
    }

    public bool IsDeviceFeedbackEnabled(string deviceName) {
        return senders.ContainsKey(deviceName) && senders[deviceName].isRegistered;
    }

    public void EnableRaycastHit(string deviceName) {
        if (senders.ContainsKey(deviceName) == false) { return; }

        var feedback = senders[deviceName] as AirVRInputDeviceFeedback;
        if (feedback != null) {
            feedback.EnableRaycastHit(true);
        }
    }

    public void DisableRaycastHit(string deviceName) {
        if (senders.ContainsKey(deviceName) == false) { return; }

        var feedback = senders[deviceName] as AirVRInputDeviceFeedback;
        if (feedback != null) {
            feedback.EnableRaycastHit(false);
        }
    }

    public void DisableAllRaycastHitFeedbacks() {
        foreach (var device in senders.Values) {
            var feedback = device as AirVRInputDeviceFeedback;
            if (feedback == null) { continue; }

            feedback.EnableRaycastHit(false);
        }
    }

    public void PendRaycastHitResult(string deviceName, byte controlID, Vector3 rayOrigin, Vector3 hitPosition, Vector3 hitNormal) {
        if (senders.ContainsKey(deviceName) == false) { return; }

        var feedback = senders[deviceName] as AirVRInputDeviceFeedback;
        if (feedback != null) {
            feedback.PendRaycastHitResult(rayOrigin, hitPosition, hitNormal);
        }
    }

    // implements AirVRInputStreaming
    protected override float maxSendingRatePerSec {
        get {
            return 90.0f;
        }
    }

    protected override long GetInputRecvTimestampImpl() {
        Assert.IsNotNull(owner);
        return ocs_GetInputRecvTimestamp(owner.playerID);
    }

    protected override void BeginPendInputImpl(ref long timestamp) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        ocs_BeginPendInput(owner.playerID, ref timestamp);
    }

    protected override void UnregisterInputSenderImpl(byte id) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        ocs_UnregisterInputSender(owner.playerID, id);
    }

    protected override void PendInputTransformImpl(byte deviceID, byte controlID, Vector3 position, Quaternion orientation, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputRaycastHitImpl(byte deviceID, byte controlID, Vector3 worldRayOrigin, Vector3 worldHitPosition, Vector3 worldHitNormal, byte policy) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        ocs_PendInputRaycastHit(owner.playerID, deviceID, controlID, worldRayOrigin.x, worldRayOrigin.y, worldRayOrigin.z,
                                worldHitPosition.x, worldHitPosition.y, worldHitPosition.z, worldHitNormal.x, worldHitNormal.y, worldHitNormal.z, policy);
    }

    protected override void PendInputFloat4Impl(byte deviceID, byte controlID, Vector4 value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputFloat3Impl(byte deviceID, byte controlID, Vector3 value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputFloat2Impl(byte deviceID, byte controlID, Vector2 value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputFloatImpl(byte deviceID, byte controlID, float value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputByteImpl(byte deviceID, byte controlID, byte value, byte policy) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        ocs_PendInputByte(owner.playerID, deviceID, controlID, value, policy);
    }

    protected override bool GetInputTransformImpl(byte deviceID, byte controlID, ref Vector3 position, ref Quaternion orientation) {
        Assert.IsNotNull(owner);
        return ocs_GetInputTransform(owner.playerID, deviceID, controlID, ref position.x, ref position.y, ref position.z, ref orientation.x, ref orientation.y, ref orientation.z, ref orientation.w);
    }

    protected override bool GetInputRaycastHitImpl(byte deviceID, byte controlID, ref Vector3 worldRayOrigin, ref Vector3 worldHitPosition, ref Vector3 worldHitNormal) {
        Assert.IsTrue(false);
        return false;
    }

    protected override bool GetInputFloat4Impl(byte deviceID, byte controlID, ref Vector4 value) {
        Assert.IsNotNull(owner);
        return ocs_GetInputFloat4(owner.playerID, deviceID, controlID, ref value.x, ref value.y, ref value.z, ref value.w);
    }

    protected override bool GetInputFloat3Impl(byte deviceID, byte controlID, ref Vector3 value) {
        Assert.IsNotNull(owner);
        return ocs_GetInputFloat3(owner.playerID, deviceID, controlID, ref value.x, ref value.y, ref value.z);
    }

    protected override bool GetInputFloat2Impl(byte deviceID, byte controlID, ref Vector2 value) {
        Assert.IsNotNull(owner);
        return ocs_GetInputFloat2(owner.playerID, deviceID, controlID, ref value.x, ref value.y);
    }

    protected override bool GetInputFloatImpl(byte deviceID, byte controlID, ref float value) {
        Assert.IsNotNull(owner);
        return ocs_GetInputFloat(owner.playerID, deviceID, controlID, ref value);
    }

    protected override bool GetInputByteImpl(byte deviceID, byte controlID, ref byte value) {
        Assert.IsTrue(false);
        return false;
    }

    protected override void SendPendingInputEventsImpl(long timestamp) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);
        ocs_SendPendingInputs(owner.playerID, timestamp);
    }

    protected override void ResetInputImpl() {
        Assert.IsTrue(owner != null && owner.isBoundToClient);
        ocs_ResetInput(owner.playerID);
    }

    private void registerInputDeviceFeedback(AirVRInputDeviceFeedback feedback) {
        int cookieTextureSize = Marshal.SizeOf(feedback.cookieTexture[0]) * feedback.cookieTexture.Length;
        IntPtr ptr = Marshal.AllocHGlobal(cookieTextureSize);

        Marshal.Copy(feedback.cookieTexture, 0, ptr, feedback.cookieTexture.Length);
        feedback.OnRegistered(ocs_RegisterInputDeviceFeedback(owner.playerID, feedback.name, ptr, cookieTextureSize, feedback.cookieDepthScaleMultiplier));

        Marshal.FreeHGlobal(ptr);
    }
}
