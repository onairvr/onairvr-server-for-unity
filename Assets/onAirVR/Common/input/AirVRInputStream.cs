﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class AirVRInputBase {
    public const int InvalidDeviceID = -1;

    public AirVRInputBase() {
        deviceID = InvalidDeviceID;
    }

    public int deviceID { get; private set; }

    public bool isRegistered {
        get {
            return deviceID != InvalidDeviceID;
        }
    }

    public virtual void OnRegistered(byte inDeviceID) {
        deviceID = inDeviceID;
    }

    public virtual void OnUnregistered() {
        deviceID = InvalidDeviceID;
    }

    public abstract string name { get; }
}

public abstract class AirVRInputSender : AirVRInputBase {
    public abstract void PendInputsPerFrame(AirVRInputStream inputStream);
}

public abstract class AirVRInputReceiver : AirVRInputBase {
    public virtual void ConfigureInputs(AirVRInputStream inputStream) { }
    public abstract void PollInputsPerFrame(AirVRInputStream inputStream);
}

public abstract class AirVRInputStream {
    private enum SendingPolicy {
        Never = 0,
        Always,
        NonzeroAlwaysZeroOnce,
        OnChange
    }

    public enum InputFilter {
        None = 0,
        PoseCAP
    }

    public class InputFilterParams {
        public int queueSize;
        public int order;
        public float predictionTime;

        public InputFilterParams(int queueSize = 3, int order = 2, float predictionTime = 0.5f) {
            this.queueSize = queueSize;
            this.order = order;
            this.predictionTime = predictionTime;
        }
    }

    public AirVRInputStream() {
        senders = new Dictionary<string, AirVRInputSender>();
        receivers = new Dictionary<string, AirVRInputReceiver>();

        _timer = new FixedRateTimer();
    }

    private bool _streaming;
    private FixedRateTimer _timer;

    protected Dictionary<string, AirVRInputSender> senders      { get; private set; }
    protected Dictionary<string, AirVRInputReceiver> receivers  { get; private set; }

    protected bool initialized {
        get {
            return _timer.isSet;
        }
    }

    protected abstract float maxSendingRatePerSec { get; }
    protected abstract void UnregisterInputSenderImpl(byte id);
    protected abstract void ConfigureInputTransformImpl(byte deviceID, byte controlID, byte filter, InputFilterParams filerParams);

    protected abstract long GetInputRecvTimestampImpl();
    protected abstract void BeginPendInputImpl(ref long timestamp);

    protected abstract void PendInputTransformImpl(byte deviceID, byte controlID, Vector3 position, Quaternion orientation, byte policy);
    protected abstract void PendInputFloat4Impl(byte deviceID, byte controlID, Vector4 value, byte policy);
    protected abstract void PendInputFloat3Impl(byte deviceID, byte controlID, Vector3 value, byte policy);
    protected abstract void PendInputFloat2Impl(byte deviceID, byte controlID, Vector2 value, byte policy);
    protected abstract void PendInputFloatImpl(byte deviceID, byte controlID, float value, byte policy);
    protected abstract void PendInputByteImpl(byte deviceID, byte controlID, byte value, byte policy);
    protected abstract void PendInputRaycastHitImpl(byte deviceID, byte controlID, Vector3 worldRayOrigin, Vector3 worldHitPosition, Vector3 worldHitNormal, byte policy);

    protected abstract bool GetInputTransformImpl(byte deviceID, byte controlID, ref Vector3 position, ref Quaternion orientation);
    protected abstract bool GetInputFloat4Impl(byte deviceID, byte controlID, ref Vector4 value);
    protected abstract bool GetInputFloat3Impl(byte deviceID, byte controlID, ref Vector3 value);
    protected abstract bool GetInputFloat2Impl(byte deviceID, byte controlID, ref Vector2 value);
    protected abstract bool GetInputFloatImpl(byte deviceID, byte controlID, ref float value);
    protected abstract bool GetInputByteImpl(byte deviceID, byte controlID, ref byte value);
    protected abstract bool GetInputRaycastHitImpl(byte deviceID, byte controlID, ref Vector3 worldRayOrigin, ref Vector3 worldHitPosition, ref Vector3 worldHitNormal);

    protected abstract void SendPendingInputEventsImpl(long timestamp);
    protected abstract void ResetInputImpl();

    public long timestamp {
        get {
            return GetInputRecvTimestampImpl();
        }
    }

    public virtual void Init() {
        _timer.Set(maxSendingRatePerSec);
    }

    public virtual void Start() {
        _streaming = true;

        foreach (var receiver in receivers.Values) {
            receiver.ConfigureInputs(this);
        }
    }

    public virtual void Stop() {
        _streaming = false;

        ResetInputImpl();
    }

    public virtual void Cleanup() {
        _timer.Reset();
        _streaming = false;

        foreach (var key in receivers.Keys) {
            receivers[key].OnUnregistered();
        }

        foreach (var key in senders.Keys) {
            if (senders[key].isRegistered) {
                UnregisterInputSenderImpl((byte)senders[key].deviceID);
                senders[key].OnUnregistered();
            }
        }
    }

    public void HandleRemoteInputDeviceRegistered(string deviceName, byte deviceID) {
        if (receivers.ContainsKey(deviceName) && receivers[deviceName].isRegistered == false) {
            receivers[deviceName].OnRegistered(deviceID);
        }
    }

    public void HandleRemoteInputDeviceUnregistered(byte deviceID) {
        foreach (var key in receivers.Keys) {
            if (receivers[key].isRegistered && receivers[key].deviceID == (int)deviceID) {
                receivers[key].OnUnregistered();
            }
        }
    }

    public void ConfigureInputTransform(AirVRInputReceiver receiver, byte controlID, InputFilter filter, InputFilterParams filterParams) {
        Assert.IsTrue(receiver.isRegistered);

        ConfigureInputTransformImpl((byte)receiver.deviceID, controlID, (byte)filter, filterParams);
    }

    public void PendTouch(AirVRInputSender sender, byte controlID, Vector2 position, bool touch) {
        Assert.IsTrue(sender.isRegistered);

        var value = new Vector3(position.x, position.y, touch ? 1.0f : 0.0f);
        PendInputFloat3Impl((byte)sender.deviceID, controlID, value, (byte)SendingPolicy.NonzeroAlwaysZeroOnce);
    }

    public void GetTouch(AirVRInputReceiver receiver, byte controlID, out Vector2 position, out float touch) {
        if (receiver.isRegistered) {
            var value = Vector3.zero;
            if (GetInputFloat3Impl((byte)receiver.deviceID, controlID, ref value)) {
                position = new Vector2(value.x, value.y);
                touch = value.z;
                return;
            }
        }

        position = Vector2.zero;
        touch = 0.0f;
    }

    public void PendQuaternion(AirVRInputSender sender, byte controlID, Quaternion value) {
        Assert.IsTrue(sender.isRegistered);

        PendInputFloat4Impl((byte)sender.deviceID, controlID, new Vector4(value.x, value.y, value.z, value.w), (byte)SendingPolicy.Always);
    }

    public Quaternion GetQuaternion(AirVRInputReceiver receiver, byte controlID) {
        Vector4 result = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        if (receiver.isRegistered) {
            if (GetInputFloat4Impl((byte)receiver.deviceID, controlID, ref result) == false) {
                result = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            }
        }
        return new Quaternion(result.x, result.y, result.z, result.w);
    }

    public void PendVector3D(AirVRInputSender sender, byte controlID, Vector3 value) {
        Assert.IsTrue(sender.isRegistered);

        PendInputFloat3Impl((byte)sender.deviceID, controlID, value, (byte)SendingPolicy.Always);
    }

    public Vector3 GetVector3D(AirVRInputReceiver receiver, byte controlID) {
        Vector3 result = Vector3.zero;
        if (receiver.isRegistered) {
            if (GetInputFloat3Impl((byte)receiver.deviceID, controlID, ref result) == false) {
                result = Vector3.zero;
            }
        }
        return result;
    }

    public void PendVector2D(AirVRInputSender sender, byte controlID, Vector2 value) {
        Assert.IsTrue(sender.isRegistered);

        PendInputFloat2Impl((byte)sender.deviceID, controlID, value, (byte)SendingPolicy.Always);
    }

    public Vector2 GetVector2D(AirVRInputReceiver receiver, byte controlID) {
        Vector2 result = Vector2.zero;
        if (receiver.isRegistered) {
            if (GetInputFloat2Impl((byte)receiver.deviceID, controlID, ref result) == false) {
                result = Vector2.zero;
            }
        }
        return result;
    }

    public void PendAxis2D(AirVRInputSender sender, byte controlID, Vector2 value) {
        Assert.IsTrue(sender.isRegistered);

        PendInputFloat2Impl((byte)sender.deviceID, controlID, value, (byte)SendingPolicy.NonzeroAlwaysZeroOnce);
    }

    public Vector2 GetAxis2D(AirVRInputReceiver receiver, byte controlID) {
        return GetVector2D(receiver, controlID);
    }

    public void PendAxis(AirVRInputSender sender, byte controlID, float value) {
        Assert.IsTrue(sender.isRegistered);

        PendInputFloatImpl((byte)sender.deviceID, controlID, value, (byte)SendingPolicy.NonzeroAlwaysZeroOnce);
    }

    public float GetAxis(AirVRInputReceiver receiver, byte controlID) {
        float result = 0.0f;
        if (receiver.isRegistered) {
            if (GetInputFloatImpl((byte)receiver.deviceID, controlID, ref result) == false) {
                result = 0.0f;
            }
        }
        return result;
    }

    public void PendButton(AirVRInputSender sender, byte controlID, float value) {
        PendAxis(sender, controlID, value);
    }

    public float GetButton(AirVRInputReceiver receiver, byte controlID) {
        return GetAxis(receiver, controlID);
    }

    public void PendButton(AirVRInputSender sender, byte controlID, bool value) {
        PendAxis(sender, controlID, value ? 1.0f : 0.0f);
    }

    public void PendTransform(AirVRInputSender sender, byte controlID, Vector3 position, Quaternion orientation) {
        Assert.IsTrue(sender.isRegistered);

        PendInputTransformImpl((byte)sender.deviceID, controlID, position, orientation, (byte)SendingPolicy.Always);
    }

    public void GetTransform(AirVRInputReceiver receiver, byte controlID, out Vector3 position, out Quaternion orientation) {
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (receiver.isRegistered) {
            if (GetInputTransformImpl((byte)receiver.deviceID, controlID, ref pos, ref rot) == false) {
                pos = Vector3.zero;
                rot = Quaternion.identity;
            }
        }

        position = pos;
        orientation = rot;
    }

    public void PendRaycastHit(AirVRInputSender sender, byte controlID, Vector3 rayOrigin, Vector3 hitPosition, Vector3 hitNormal) {
        Assert.IsTrue(sender.isRegistered);

        PendInputRaycastHitImpl((byte)sender.deviceID, controlID, rayOrigin, hitPosition, hitNormal, (byte)SendingPolicy.NonzeroAlwaysZeroOnce);
    }

    public void GetRaycastHit(AirVRInputReceiver receiver, byte controlID, out Vector3 rayOrigin, out Vector3 hitPosition, out Vector3 hitNormal) {
        Vector3 resultRayOrigin = Vector3.zero;
        Vector3 resultHitPosition = Vector3.zero;
        Vector3 resultHitNormal = Vector3.zero;

        if (receiver.isRegistered) {
            if (GetInputRaycastHitImpl((byte)receiver.deviceID, controlID, ref resultRayOrigin, ref resultHitPosition, ref resultHitNormal) == false) {
                resultRayOrigin = resultHitPosition = resultHitNormal = Vector3.zero;
            }
        }

        rayOrigin = resultRayOrigin;
        hitPosition = resultHitPosition;
        hitNormal = resultHitNormal;
    }

    public void PendState(AirVRInputSender sender, byte controlID, byte value) {
        Assert.IsTrue(sender.isRegistered);

        PendInputByteImpl((byte)sender.deviceID, controlID, value, (byte)SendingPolicy.OnChange);
    }

    public void UpdateReceivers() {
        foreach (var key in receivers.Keys) {
            receivers[key].PollInputsPerFrame(this);
        }
    }

    public void UpdateSenders() {
        if (_streaming) {
            _timer.UpdatePerFrame();

            if (_timer.expired) {
                long timestamp = 0;
                BeginPendInputImpl(ref timestamp);

                foreach (var key in senders.Keys) {
                    senders[key].PendInputsPerFrame(this);
                }
                SendPendingInputEventsImpl(timestamp);
            }
        }
    }
}
